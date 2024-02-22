using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using JackTheVideoRipper.extensions;
using JackTheVideoRipper.framework;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.DataStructures;
using JackTheVideoRipper.views;

namespace JackTheVideoRipper.models.processes;

public class ProcessPool
{
    #region Data Members

    private readonly ProcessTable _processTable = new();
    private readonly ConcurrentQueue<IProcessUpdateRow> _processQueue = new();
    private readonly ConcurrentDictionary<string, IProcessUpdateRow> _pausedProcesses = new();
    private readonly IndexableQueue<IProcessUpdateRow> _resumeProcessQueue = new();
    private readonly ProcessTable _runningProcesses = new();
    private readonly ConcurrentQueue<IProcessUpdateRow> _onDeckProcessQueue = new();
    private readonly ConcurrentHashSet<IProcessUpdateRow> _finishedProcesses = new();
    private readonly ConcurrentList<MediaWorker> _mediaWorkers = new(5);
    
    private readonly Task[] _clearTasks;

    public static readonly ErrorLogger ErrorLogger = new();

    private bool _updating;
    private bool _fillingQueue;

    #endregion

    #region Constructor

    public ProcessPool()
    {
        _clearTasks = new Task[]
        {
            new(_runningProcesses.Clear),
            new(_finishedProcesses.Clear),
            new(_pausedProcesses.Clear),
            new(_resumeProcessQueue.Clear),
            new(_processQueue.Clear),
            new(_processTable.Clear)
        };
    }

    #endregion

    #region Events

    public event Action<IProcessUpdateRow> ProcessCompleted = delegate { };
    
    public event Action ProcessStarted = delegate { };

    #endregion

    #region Properties

    public static int MaxConcurrentDownloads => Settings.Data.MaxConcurrentDownloads;

    public bool AnyActive => AnyRunning || AnyQueued || AnyPaused;

    public bool AnyRunning => _runningProcesses.HasCached;

    public bool AnyQueued => !_processQueue.IsEmpty;

    public bool AnyPaused => !_resumeProcessQueue.Empty();
    
    public bool QueueEmpty => _processQueue.IsEmpty && _resumeProcessQueue.Empty();
    
    public bool AtCapacity => RunningCount == MaxConcurrentDownloads;
    
    private bool NoneOnDeck => _onDeckProcessQueue.IsEmpty;

    private IProcessUpdateRowEnumerable Processes => _processTable.Processes;
    
    public IProcessUpdateRowEnumerable RunningProcesses => _runningProcesses.Cached;
    
    public IProcessUpdateRowEnumerable CompletedProcesses => GetWhereStatus(ProcessStatus.Completed);
    
    public IProcessUpdateRowEnumerable SucceededProcesses => GetWhereStatus(ProcessStatus.Succeeded);
    
    public IProcessUpdateRowEnumerable FailedProcesses => GetWhereStatus(ProcessStatus.Error);

    public IEnumerable<string> FailedUrls => FailedProcesses.Select(p => p.Url);
    
    public IEnumerable<string> Urls => Processes.Select(p => p.Url);
    
    public IEnumerable<int> ActiveProcessIds => RunningProcesses.Select(p => p.ProcessId);

    public int ActiveCount => _runningProcesses.Count + 
                              _processQueue.Count + 
                              _onDeckProcessQueue.Count + 
                              _resumeProcessQueue.Count;

    public int RunningCount => _runningProcesses.Count;

    public int PausedCount => _pausedProcesses.Count;

    public int TotalCount => _processTable.Count;

    public int[] ProcessCounts
    {
        get
        {
            return new[]
            {
                ActiveCount,    //< Active
                RunningCount,   //< Running
                PausedCount,    //< Paused
                TotalCount      //< Total
            };
        }
    }

    public string PoolStatus
    {
        get
        {
            if (AnyRunning)
                return "Processing Media";
            if (AnyQueued)
                return "Awaiting Process";
            if (QueueEmpty)
                return "Idle";
            return string.Empty;
        }
    }

    private IProcessUpdateRow? NextProcess
    {
        get
        {
            if (QueueEmpty)
                return null;
            
            if (AnyPaused)
                return _resumeProcessQueue.Dequeue();

            if (AnyQueued && _processQueue.TryDequeue(out IProcessUpdateRow? result))
                return result;

            return null;
        }
    }

    #endregion

    #region Worker Methods

    private void InitializeWorkers()
    {
        foreach (MediaWorker worker in _mediaWorkers)
        {
            worker.InitializeWorker();
            worker.ProgressChanged += OnUpdateWorker;
            worker.RunWorkerCompleted += OnWorkerComplete;
        }
    }

    private void OnUpdateWorker(object? sender, ProgressChangedEventArgs args)
    {
        // Progress is passed as an integer, all other decimal places will be truncated
        float progress = args.ProgressPercentage / MediaWorker.PROGRESS_PRECISION_FACTOR;

        if (args.UserState is not ProcessUpdateArgs updateArgs)
            return;
        
        // TODO: Check if updateArgs.Completed
        
        // TODO: Send progress/updateArgs to the view
        
        // TODO: Queue updates... use dictionary and push to when updating, then every tick update the view all together,
        //          when a new update hits, only save the latest
    }

    private void OnWorkerComplete(object? sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
    {
        if (sender is not MediaWorker worker)
            return;
        
        worker.Claimed = false;
        
        // TODO: Final update on the view to set at 100% and get the size where applicable...
    }

    // TODO: Call from the StartProcess method
    private async Task RunTaskOnWorker(ProcessRunner processRunner)
    {
        (await GetNextAvailableWorker()).RunWorkerAsync(processRunner);
    }

    private async Task<MediaWorker> GetNextAvailableWorker()
    {
        return await _mediaWorkers.Next(w => w is { Claimed: false, IsBusy: false }, 500);
    }

    #endregion

    #region Public Methods

    public void Initialize()
    {
        InitializeWorkers();
    }

    public async Task Update()
    {
        if (!AtCapacity && !QueueEmpty)
            await UpdateQueue();
        
        // Skip update if we have no running or we're currently updating
        if (!AnyRunning || _updating)
            return;
        
        _updating = true;
        await RunningProcesses.Update();
        _updating = false;
    }

    public void OnCompleteProcess(IProcessRunner processRunner)
    {
        if (processRunner is not IProcessUpdateRow processUpdateRow)
            return;
        FinishProcess(processUpdateRow);
    }

    public IViewItem? Remove(string tag)
    {
        if (GetProcess(tag) is not { } processUpdateRow)
            return default;

        Remove(processUpdateRow);

        return processUpdateRow.ViewItem;
    }
    
    public void Remove(IProcessUpdateRow processUpdateRow)
    {
        switch (processUpdateRow.ProcessStatus)
        {
            default:
            case ProcessStatus.Failed:
            case ProcessStatus.Completed:
            case ProcessStatus.Created:
                return;
            case ProcessStatus.Running:
                StopProcess(processUpdateRow);
                return;
            case ProcessStatus.Succeeded:
            case ProcessStatus.Error:
            case ProcessStatus.Stopped:
            case ProcessStatus.Cancelled:
                RemoveFinished(processUpdateRow);
                return;
            case ProcessStatus.Queued:
                RemoveQueued(processUpdateRow);
                return;
            case ProcessStatus.Paused:
                RemovePaused(processUpdateRow);
                return;
        }
    }

    public async Task Refresh()
    {
        await UpdateQueue();
    }

    public async Task UpdateQueue()
    {
        FillQueue();

        while (!AtCapacity && !NoneOnDeck)
        {
            await DispatchFromQueue();
            await Task.Delay(Global.Configurations.PROCESS_POOL_UPDATE_FREQUENCY);
        }
    }

    // Move On Deck processes to running
    private async Task DispatchFromQueue()
    {
        if (!_onDeckProcessQueue.TryDequeue(out IProcessUpdateRow? updateRow))
            throw new ProcessPoolException("Failed to dispatch on deck process");
        await StartProcess(updateRow);
    }
    
    public void QueueProcess(IProcessUpdateRow processUpdateRow, IViewItemAction queueCallback)
    {
        if (!_processTable.TryAdd(processUpdateRow))
            throw new ProcessPoolException(string.Format(Messages.FailedToAddProcess, processUpdateRow.Tag));
        _processQueue.Enqueue(processUpdateRow);
        processUpdateRow.Enqueue();
        queueCallback(processUpdateRow.ViewItem);
    }
    
    public void FindOrphanedProcesses()
    {
        Parallel.ForEach(ActiveProcessIds.Select(Process.GetProcessById), process =>
        {
            Output.WriteLine($"Found process with Id: {process.Id}");
        });
    }

    #endregion

    #region Private Methods

    private void FillQueue()
    {
        if (_fillingQueue || QueueEmpty || _onDeckProcessQueue.Count == MaxConcurrentDownloads)
            return;

        _fillingQueue = true;
        for (int i = _onDeckProcessQueue.Count; i <= MaxConcurrentDownloads; i++)
        {
            if (NextProcess is not { } nextProcess)
                break;
            _onDeckProcessQueue.Enqueue(nextProcess);
        }
        _fillingQueue = false;
    }

    private async ValueTask StartProcess(IProcessUpdateRow processUpdateRow, CancellationToken? cancellationToken = null)
    {
        RunProcess(processUpdateRow);

        // Resume if we are Starting a Paused/Suspended Process
        if (processUpdateRow.Paused)
        {
            processUpdateRow.Resume();
            ProcessStarted();
            return;
        }

        // Starting Process Succeeded
        if (await processUpdateRow.Start())
        {
            ProcessStarted();
            return;
        }
        
        // Failed to Start Process
        StopProcess(processUpdateRow);
        throw new ProcessPoolException($"Failed to start process {processUpdateRow.Tag}");
    }

    #endregion

    #region State Machine Operations

    private void FinishProcess(IProcessUpdateRow processUpdateRow)
    {
        StopProcess(processUpdateRow);
        _finishedProcesses.Add(processUpdateRow);
        ProcessCompleted(processUpdateRow);
    }

    public bool RetryProcess(string tag)
    {
        return GetProcess(tag) is { } processUpdateRow && RetryProcess(processUpdateRow);
    }
    
    public bool RetryProcess(IProcessUpdateRow processUpdateRow)
    {
        if (!_finishedProcesses.Remove(processUpdateRow))
            return false;
        _processQueue.Enqueue(processUpdateRow);
        processUpdateRow.Retry();
        return true;
    }
    
    public void PauseProcess(string tag)
    {
        if (GetProcess(tag) is not { } processUpdateRow)
            return;
        
        PauseProcess(processUpdateRow);
    }
    
    public void PauseProcess(IProcessUpdateRow processUpdateRow)
    {
        StopProcess(processUpdateRow);
        _pausedProcesses.TryAdd(processUpdateRow.Tag, processUpdateRow);
        processUpdateRow.Pause();
    }
    
    public void ResumeProcess(string tag)
    {
        if (GetProcess(tag) is not { } processUpdateRow)
            return;
        
        ResumeProcess(processUpdateRow);
    }
    
    public void ResumeProcess(IProcessUpdateRow processUpdateRow)
    {
        _pausedProcesses.TryRemove(processUpdateRow.Tag, out _);
        _resumeProcessQueue.Add(processUpdateRow);
    }

    private void RunProcess(IProcessUpdateRow processUpdateRow)
    {
        _runningProcesses.Add(processUpdateRow);
    }

    private bool StopProcess(IProcessUpdateRow processUpdateRow)
    {
        return _runningProcesses.Remove(processUpdateRow);
    }
    
    private void RemoveFinished(IProcessUpdateRow processUpdateRow)
    {
        _finishedProcesses.Remove(processUpdateRow);
    }
    
    private static void RemoveQueued(IProcessRunner processUpdateRow)
    {
        processUpdateRow.Cancel();
    }

    private void RemovePaused(IProcessUpdateRow processUpdateRow)
    {
        _pausedProcesses.TryRemove(processUpdateRow.Tag, out _);
        processUpdateRow.Cancel();
    }

    #endregion

    #region Querying

    public bool Exists(IProcessUpdateRow processUpdateRow)
    {
        return _processTable.Contains(processUpdateRow.Tag);
    }
    
    public bool Exists(string tag)
    {
        return tag.HasValue() && _processTable.Contains(tag);
    }

    public IEnumerable<T> GetOfType<T>()
    {
        return GetWhere(row => row is T).Cast<T>();
    }

    public IProcessUpdateRowEnumerable GetWhere(Func<IProcessUpdateRow, bool> predicate)
    {
        return Processes.Where(predicate);
    }

    public IProcessUpdateRow? GetProcess(string tag)
    {
        return Exists(tag) ? _processTable[tag] : default;
    }
    
    public bool TryGetProcess(string tag, out IProcessUpdateRow? processUpdateRow)
    {
        if (Exists(tag))
            return _processTable.TryGet(tag, out processUpdateRow);
        processUpdateRow = default;
        return false;
    }

    public IProcessUpdateRowEnumerable GetWhereStatus(ProcessStatus processStatus)
    {
        return GetWhere(p => p.ProcessStatus == processStatus);
    }
    
    public IProcessUpdateRowEnumerable GetWhereStatus(params ProcessStatus[] statuses)
    {
        return GetWhereStatus(statuses.Aggregate(0u, (i, status) => i | (uint) status));
    }
    
    public IProcessUpdateRowEnumerable GetWhereStatus(uint status)
    {
        return GetWhere(p => ((uint)p.ProcessStatus & status) > 0);
    }
    
    public IProcessUpdateRowEnumerable GetFinished(ProcessStatus processStatus)
    {
        return _finishedProcesses.Where(p => p.ProcessStatus == processStatus);
    }
    
    private IViewItemEnumerable RemoveWhere(ProcessStatus processStatus, IViewItemAction? removeCallback = null)
    {
        IViewItem[] results = RemoveAll(processStatus).SelectViewItems().ToArray();
        if (removeCallback is not null)
            Parallel.ForEach(results, removeCallback);
        return results;
    }

    #endregion

    #region Bulk Actions

    public void PauseAll()
    {
        Parallel.ForEach(RunningProcesses, PauseProcess);
    }

    public void ResumeAll()
    {
        Parallel.ForEach(_pausedProcesses.Values, ResumeProcess);
    }

    public void KillAllRunning()
    {
        // kill all processes
        RunningProcesses.Kill();
    }

    public void StopAll()
    {
        RunningProcesses.Stop();
        _processQueue.Stop();
    }
    
    public void ClearAll()
    {
        StopAll();
        Task.WhenAll(_clearTasks);
    }

    public IViewItemEnumerable RemoveSucceeded(IViewItemAction? removeCallback = null)
    {
        return RemoveWhere(ProcessStatus.Succeeded, removeCallback);
    }
    
    public IViewItemEnumerable RemoveFailed(IViewItemAction? removeCallback = null)
    {
        return RemoveWhere(ProcessStatus.Error, removeCallback);
    }
    
    public bool RetryAllProcesses()
    {
        IProcessUpdateRow[] erroredProcesses = GetWhereStatus(ProcessStatus.Error).ToArray();
        List<bool> results = new(erroredProcesses.Length);

        void AddResult(IProcessUpdateRow process)
        {
            bool result = RetryProcess(process);
            lock (results) { results.Add(result); }
        }

        Parallel.ForEach(erroredProcesses, AddResult);
        
        return results.Any();
    }
    
    public IProcessUpdateRowEnumerable RemoveAll(ProcessStatus processStatus)
    {
        IProcessUpdateRowEnumerable processes = Array.Empty<IProcessUpdateRow>();
        
        switch (processStatus)
        {
            default:
            case ProcessStatus.Created:
                break;
            case ProcessStatus.Running:
                processes = RunningProcesses.ToArray();
                _runningProcesses.Remove(processes);
                break;
            case ProcessStatus.Succeeded:
            case ProcessStatus.Error:
            case ProcessStatus.Stopped:
            case ProcessStatus.Cancelled:
                processes = GetFinished(processStatus).ToArray();
                _finishedProcesses.Remove(processes);
                break;
            case ProcessStatus.Queued: // TODO:
                processes = GetWhereStatus(processStatus).ToArray();
                break;
            case ProcessStatus.Paused:
                processes = GetWhereStatus(processStatus).ToArray();
                Parallel.ForEach(processes, RemovePaused);
                break;
        }
        
        return processes;
    }

    #endregion

    #region Embedded Types

    public class ProcessPoolException : Exception
    {
        public ProcessPoolException()
        {
        }
        
        public ProcessPoolException(string message) : base(message)
        {
        }
        
        public ProcessPoolException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    #endregion
}