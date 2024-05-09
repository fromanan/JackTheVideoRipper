using System.Diagnostics;
using JackTheVideoRipper.extensions;
using JackTheVideoRipper.framework;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.containers;
using JackTheVideoRipper.models.data_structures;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.models.parameters;
using JackTheVideoRipper.models.processes;
using JackTheVideoRipper.models.rows;
using JackTheVideoRipper.modules;
using JackTheVideoRipper.Properties;
using JackTheVideoRipper.views;

namespace JackTheVideoRipper.models;

public class MediaManager
{
    #region Data Members

    private readonly ProcessPool _processPool = new();

    private readonly ConcurrentHashSet<string> _downloadedUrls = new();

    #endregion

    #region Properties

    public IProcessUpdateRow? GetRow(string tag) => _processPool.GetProcess(tag);

    public IProcessUpdateRow? Selected => _processPool.GetProcess(Ripper.SelectedTag) ?? null;
    
    public IEnumerable<string> DownloadedUrls => _downloadedUrls;

    #endregion

    #region Events

    public event Action QueueUpdated = delegate { };

    public event IViewItemAction ProcessAdded = delegate { };
    
    public event IViewItemEnumerableAction ProcessesAdded = delegate { };

    public event IViewItemAction ProcessRemoved = delegate { };
    
    public event IViewItemEnumerableAction ProcessesRemoved = delegate { };
    
    public event Action<IEnumerable<ProcessUpdateArgs>> ProcessPoolUpdated = delegate { };

    #endregion

    #region Constructor

    public MediaManager()
    {
        Core.ShutdownEvent += OnProgramShutdown;
        InitializeProcessPool();
    }

    #endregion

    #region Bulk Actions

    public void ClearAll()
    {
        _processPool.ClearAll();
    }

    public void StopAll()
    {
        _processPool.StopAll();
    }

    public void RetryAll()
    {
        _processPool.RetryAllProcesses();
    }

    public async Task UpdateListItemRows()
    {
        await _processPool.Update();
    }

    public void PauseAll()
    {
        _processPool.PauseAll();
    }

    public void ResumeAll()
    {
        _processPool.ResumeAll();
    }

    #endregion

    #region Public Methods

    public bool WaitForNextDispatch(int millisecondsTimeout)
    {
        return _processPool.WaitForNextDispatch(millisecondsTimeout);
    }

    public async Task Refresh()
    {
        await _processPool.Refresh();
    }

    public string GetStatus()
    {
        return _processPool.PoolStatus;
    }

    public int[] GetProcessCounts()
    {
        return _processPool.ProcessCounts;
    }

    public void RemoveSucceeded() => ProcessesRemoved(_processPool.RemoveSucceeded());

    public void RemoveFailed() => ProcessesRemoved(_processPool.RemoveFailed());

    private void AddRow(IMediaItem mediaItem, ProcessRowType processRowType)
    {
        switch (processRowType)
        {
            case ProcessRowType.Download:
                // TODO: Check for option to skip or prompt for overwrite too
                if (_downloadedUrls.Contains(mediaItem.Url))
                    return;
                _downloadedUrls.Add(mediaItem.Url);
                AddProcess(new DownloadProcessUpdateRow(mediaItem, _processPool.OnCompleteProcess));
                break;
            case ProcessRowType.Compress:
                AddProcess(new CompressProcessUpdateRow(mediaItem, _processPool.OnCompleteProcess));
                break;
            case ProcessRowType.Convert:
                //AddProcess(new ConversionProcessUpdateRow(mediaItem, _processPool.OnCompleteProcess));
                break;
            case ProcessRowType.Recode:
                //AddProcess(new RecodeProcessUpdateRow(mediaItem, _processPool.OnCompleteProcess));
                break;
            case ProcessRowType.Repair:
                //AddProcess(new RepairProcessUpdateRow(mediaItem, _processPool.OnCompleteProcess));
                break;
            case ProcessRowType.Extract:
                AddProcess(new ExtractProcessUpdateRow(mediaItem, _processPool.OnCompleteProcess));
                break;
            default:
                Modals.Warning(string.Format(Messages.UnsupportedProcess, processRowType.ToString().WrapQuotes()),
                    Captions.UnsupportedTool);
                break;
        }
    }
    
    private void OnProgramShutdown()
    {
        History.Data.DownloadedUrls = DownloadedUrls.ToArray();
    }

    private void AddProcess(IProcessUpdateRow processUpdateRow)
    {
        _processPool.QueueProcess(processUpdateRow, ProcessAdded);
    }
    
    private async ValueTask QueueProcessAsync(IMediaItem row, ProcessRowType processRowType, 
        CancellationToken? cancellationToken = null)
    {
        await Threading.RunInMainContext(AddProcessTask, cancellationToken);
        return;

        void AddProcessTask()
        {
            AddRow(row, processRowType);
        }
    }

    public IEnumerable<DownloadMediaItem> FilterExistingUrls(IEnumerable<DownloadMediaItem> items)
    {
        return items.Where(UrlExists);
    }

    public async Task DownloadBatch(IEnumerable<string> urls)
    {
        await DownloadBatch(urls.MergeReturn().ReplaceLineEndings());
    }
    
    public async Task DownloadBatch(string urlString = "")
    {
        if (FrameNewMediaBatch.GetMedia(urlString) is not { } items)
            return;
        
        await Parallel.ForEachAsync(FilterExistingUrls(items), QueueDownloadTask);
        
        QueueUpdated();
        return;

        async ValueTask QueueDownloadTask(DownloadMediaItem row, CancellationToken token)
        {
            await QueueProcessAsync(row, ProcessRowType.Download, token);
        }
    }

    public async Task BatchDocument()
    {
        if (FileSystem.ReadFileUsingDialog() is not { } fileContent)
            return;
        await DownloadBatch(Import.GetAllUrlsFromPayload(fileContent));
    }

    public async Task BatchPlaylist()
    {
        if (await FrameImportPlaylist.GetMetadata() is not { } links)
            return;
        await DownloadBatch(links);
    }

    public async Task DownloadMediaDialog(MediaType type)
    {
        if (Settings.Data.SimpleDownloads)
        {
            await GetNewMediaSimple(type);
        }
        else
        {
            await GetNewMedia(type);
        }
    }
    
    private bool UrlExists(DownloadMediaItem item)
    {
        return !DownloadedUrls.Contains(item.Url);
    }

    private async Task GetNewMedia(MediaType type)
    {
        if (FrameNewMedia.GetMedia(type) is not { } mediaItemRow)
            return;
        await QueueProcessAsync(mediaItemRow, ProcessRowType.Download);
    }

    private async Task GetNewMediaSimple(MediaType type)
    {
        if (FrameNewMediaSimple.GetMedia(type) is not { } mediaItemRow)
            return;
        await QueueProcessAsync(mediaItemRow, ProcessRowType.Download);
    }

    public void RetryProcess(string tag)
    {
        _processPool.RetryProcess(tag);
        QueueUpdated();
    }
    
    public void RetryProcess(IProcessUpdateRow processUpdateRow)
    {
        if (!processUpdateRow.Failed)
            return;
        _processPool.RetryProcess(processUpdateRow);
        QueueUpdated();
    }

    public void RemoveProcess(string tag)
    {
        if (!_processPool.TryGetProcess(tag, out IProcessUpdateRow? process) || process is null)
            return;
        RemoveProcess(process);
    }
    
    public void RemoveProcess(IProcessUpdateRow processUpdateRow)
    {
        if (processUpdateRow.Running)
            processUpdateRow.Stop();
        processUpdateRow.Detach();
        _processPool.Remove(processUpdateRow);
        ProcessRemoved(processUpdateRow.ViewItem);
    }
    
    public void PauseProcess(string tag)
    {
        if (!_processPool.TryGetProcess(tag, out IProcessUpdateRow? result) || result is null)
            return;
        PauseProcess(result);
    }
    
    public void PauseProcess(IProcessUpdateRow processUpdateRow)
    {
        if (!processUpdateRow.Running || processUpdateRow.Paused)
            return;
        processUpdateRow.Pause();
        QueueUpdated();
    }

    public void ResumeProcess(string tag)
    {
        if (!_processPool.TryGetProcess(tag, out IProcessUpdateRow? result) || result is null)
            return;
        ResumeProcess(result);
    }
    
    public void ResumeProcess(IProcessUpdateRow processUpdateRow)
    {
        if (!processUpdateRow.Paused)
            return;
        processUpdateRow.Resume();
        QueueUpdated();
    }

    public void CopyFailedUrls()
    {
        FileSystem.SetClipboardText(_processPool.FailedUrls.MergeNewline());
    }
    
    public void CopyAllUrls()
    {
        FileSystem.SetClipboardText(_processPool.Urls.MergeNewline());
    }

    public async Task PerformContextAction(ContextActions contextAction)
    {
        if (Ripper.SelectedTag.IsNullOrEmpty() || Selected is null)
            return;
        
        switch (contextAction)
        {
            case ContextActions.OpenMedia:
            {
                if (Selected.Completed)
                    Common.OpenFileInMediaPlayer(Selected.Path);
                break;
            }
            case ContextActions.CopyUrl:
            {
                if (Ripper.Instance.SelectedIsType<DownloadProcessUpdateRow>())
                    Core.CopyToClipboard(Selected.Url);
                break;
            }
            case ContextActions.Delete:
            {
                if (Selected.Succeeded)
                    FileSystem.DeleteFileIfExists(Selected.Path);
                break;
            }
            case ContextActions.Stop:
            {
                if (Selected.Running)
                    StopSelectedProcess();
                break;
            }
            case ContextActions.Retry:
            {
                if (Selected.Failed)
                    RetryProcess(Selected);
                break;
            }
            case ContextActions.OpenUrl:
            {
                if (Ripper.Instance.SelectedIsType<DownloadProcessUpdateRow>())
                    Common.OpenInBrowser(Selected.Url);
                break;
            }
            case ContextActions.Reveal:
            {
                if (Selected.Succeeded)
                    FileSystem.OpenFileExplorerWithFileSelected(Selected.Path);
                break;
            }
            case ContextActions.Pause:
            {
                if (!Selected.Paused)
                    PauseProcess(Selected);
                break;
            }
            case ContextActions.Resume:
            {
                if (Selected.Paused)
                    ResumeProcess(Selected);
                break;
            }
            case ContextActions.Remove:
            {
                if (!Selected.Running || Modals.Confirmation(Messages.DeleteRunningProcess))
                    RemoveProcess(Selected);
                break;
            }
            case ContextActions.OpenConsole:
            {
                await Selected.OpenInConsole();
                break;
            }
            case ContextActions.SaveLogs:
            {
                Selected.SaveLogs();
                break;
            }
            case ContextActions.CopyCommand:
            {
                if (Selected is ProcessUpdateRow processUpdateRow)
                    FileSystem.SetClipboardText(processUpdateRow.Command);
                break;
            }
            case ContextActions.Reprocess:
            {
                NotImplemented("Reprocess");
                break;
            }
            case ContextActions.Compress:
            {
                if (Selected is ProcessUpdateRow processUpdateRow)
                    await CompressVideo(processUpdateRow.Path);
                break;
            }
            case ContextActions.Convert:
            {
                // Currently handled with a window
                break;
            }
            default:
            {
                ArgumentOutOfRangeException innerException = new(nameof(contextAction), contextAction, null);
                throw new MediaManagerException(Messages.ContextActionFailed, innerException);
            }
        }
    }
    
    public async Task DownloadFromUrl(string url)
    {
        MediaItemRow<DownloadMediaParameters> row = new(url, mediaParameters: new DownloadMediaParameters(url));
        await QueueProcessAsync(row, ProcessRowType.Download);
    }

    public async Task ConvertVideo(string filepath)
    {
        string extension = FileSystem.GetExtension(filepath);

        if (Modals.BasicDropdown(Formats.AllVideo, defaultValue: extension) is not { } choice || choice == extension)
            return;
        
        string newFilepath = FileSystem.ChangeExtension(filepath, choice);
        
        ExifData metadata = await ExifTool.GetMetadata(filepath);
        
        MediaItemRow<FfmpegParameters> row = new(title:metadata.Title, filepath: filepath,
            mediaParameters: FFMPEG.Convert(filepath, choice));
             
        await QueueProcessAsync(row, ProcessRowType.Convert);
    }

    public async Task CompressVideo(string filepath)
    {
        string newFilepath = FFMPEG.GetOutputFilename(filepath, FFMPEG.Operation.Compress);
        if (!FileSystem.WarnAndDeleteIfExists(newFilepath))
            return;
        
        //FFMPEG.VideoInformation videoInformation = await FFMPEG.ExtractVideoInformation(filepath);
             
        ExifData metadata = await ExifTool.GetMetadata(filepath);

        MediaItemRow<FfmpegParameters> row = new(title:metadata.Title, filepath: filepath,
            mediaParameters: FFMPEG.Compress(filepath));
             
        await QueueProcessAsync(row, ProcessRowType.Compress);
    }

    public async Task CompressBulk(string directoryPath)
    {
        string[] filepaths = Directory.GetFiles(directoryPath, FileFilters.VideoFiles);
        await CompressBulk(filepaths);
    }
    
    public async Task CompressBulk(IEnumerable<string> filepaths)
    {
        await Parallel.ForEachAsync(filepaths, Compress);
        return;

        async ValueTask Compress(string filepath, CancellationToken token)
        {
            await CompressVideo(filepath);
        }
    }

    public async Task RecodeVideo(string filepath)
    {
        NotImplemented(nameof(RecodeVideo));
        MediaItemRow<FfmpegParameters> row = new(filepath: filepath, mediaParameters: FFMPEG.Recode(filepath));
        await QueueProcessAsync(row, ProcessRowType.Recode);
    }

    public async Task ExtractAudio(string filepath)
    {
        MediaItemRow<FfmpegParameters> row = new(filepath: filepath, mediaParameters: FFMPEG.ExtractAudio(filepath));
        await QueueProcessAsync(row, ProcessRowType.Extract);
    }

    public async Task ValidateVideo(string filepath)
    {
        NotImplemented(nameof(ValidateVideo));
    }

    public async Task AddAudio(string filepath)
    {
        NotImplemented(nameof(AddAudio));
    }

    private static void NotImplemented(string name)
    {
        throw new DeveloperException(string.Format(Messages.NotImplemented, name), new NotImplementedException());
    }

    private static void Experimental(string name)
    {
        Output.WriteLine($"Warning: '{name}' is experimental and may not function exactly as intended. Proceed with caution.");
    }

    public async Task RepairVideo(string filepath)
    {
        NotImplemented(nameof(RepairVideo));
        
        // Order list of parameters for each task necessary
        IEnumerable<FfmpegParameters> repairTaskParameters = await FFMPEG.RepairVideo(filepath);

        // Rows for each process required
        IEnumerable<MediaItemRow<FfmpegParameters>> mediaItemRows = repairTaskParameters.Select(CreateRepairRow);

        foreach (MediaItemRow<FfmpegParameters> row in mediaItemRows)
        {
            Process process = FFMPEG.CreateCommand(row.ProcessParameters.ToString());
            process.OutputDataReceived += Output.WriteData;
            process.ErrorDataReceived += Output.WriteData;
            process.Start();
        }

        return;

        /*async void Repair(MediaItemRow<FfmpegParameters> row)
        {
            await QueueProcessAsync(row, ProcessRowType.Repair);
        }

        mediaItemRows.ForEach(Repair);*/

        MediaItemRow<FfmpegParameters> CreateRepairRow(FfmpegParameters parameters)
        {
            return new MediaItemRow<FfmpegParameters>(filepath: parameters.OutputFilepath, mediaParameters: parameters);
        }
    }

    #endregion

    #region Private Methods

    private void InitializeProcessPool()
    {
        _processPool.Updated += updates => ProcessPoolUpdated.Invoke(updates);
        _processPool.ProcessStarted += OnProcessStarted;
        _processPool.ProcessCompleted += OnProcessCompleted;
        _processPool.Initialize();
    }

    #endregion
    
    #region Event Handlers
    
    private void OnProcessCompleted(IProcessUpdateRow completedProcessRow)
    {
        QueueUpdated();
    }

    private void OnProcessStarted()
    {
        QueueUpdated();
    }

    public bool OnFormClosing()
    {
        if (!_processPool.AnyActive)
            return false;

        if (!Modals.ConfirmExit())
            return true;

        _processPool.KillAllRunning();
        return false;
    }

    #endregion

    #region Static Methods

    public void StopProcess(string tag)
    {
        _processPool.GetProcess(tag)?.Stop();
    }

    public void StopSelectedProcess()
    {
        Selected?.Stop();
    }

    public static void VerifyIntegrity()
    {
        if (FileSystem.SelectFile() is not { } filepath)
            return;

        string filepathQuotes = filepath.WrapQuotes();
        
        Output.WriteLine(string.Format(Messages.VerifyingFile, filepathQuotes));
        Output.WriteLine(FFMPEG.VerifyIntegrity(filepath), sendAsNotification:true);

        string logFilepath = FileSystem.TempFile;
        string result = File.ReadAllText(logFilepath).IsNullOrEmpty()
            ? string.Format(Messages.VerifyNoErrors, filepathQuotes)
            : string.Format(Messages.VerifyErrorsDetected, filepathQuotes, logFilepath.WrapQuotes());

        Output.WriteLine(result);
    }

    #endregion
    
    #region Embedded Types

    public class MediaManagerException : Exception
    {
        public MediaManagerException()
        {
        }
        
        public MediaManagerException(string message) : base(message)
        {
        }
        
        public MediaManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    #endregion
}