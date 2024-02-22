using JackTheVideoRipper.extensions;
using JackTheVideoRipper.framework;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.processes;

namespace JackTheVideoRipper.models.rows;

public abstract class ProcessUpdateRow : ProcessRunner, IProcessUpdateRow, IDynamicRow
{
    #region Data Members

    public IViewItem ViewItem { get; }

    public string Tag { get; } = Common.CreateTag();
    
    private readonly Console _console = new();

    public readonly string Filepath;

    public readonly string Filename;

    #endregion

    #region Properties
    
    private string DefaultTitle => YouTubeDL.GetDefaultTitle(Filename);

    #endregion

    #region View Item Accessors
    
    private readonly ViewCollection _viewCollection;

    public string Title
    {
        get => _viewCollection[ViewField.Title];
        set => _viewCollection[ViewField.Title] = value;
    }
        
    public string Status
    {
        get => _viewCollection[ViewField.Status];
        set => _viewCollection[ViewField.Status] = value;
    }

    public string MediaType
    {
        get => _viewCollection[ViewField.MediaType];
        set => _viewCollection[ViewField.MediaType] = value;
    }
        
    public string FileSize
    {
        get => _viewCollection[ViewField.Size];
        set => _viewCollection[ViewField.Size] = value;
    }
        
    public new string Progress
    {
        get => _viewCollection[ViewField.Progress];
        set => _viewCollection[ViewField.Progress] = value;
    }
        
    public string Speed
    {
        get => _viewCollection[ViewField.Speed];
        set => _viewCollection[ViewField.Speed] = value;
    }
        
    public string Eta
    {
        get => _viewCollection[ViewField.Eta];
        set => _viewCollection[ViewField.Eta] = value;
    }
        
    public string Url
    {
        get => _viewCollection[ViewField.Url];
        set => _viewCollection[ViewField.Url] = value;
    }
        
    public string Path
    {
        get => _viewCollection[ViewField.Path];
        set => _viewCollection[ViewField.Path] = value;
    }
        
    private Color Color
    {
        get => ViewItem.BackColor;
        set => ViewItem.BackColor = value;
    }
        
    #endregion

    #region Constructor

    protected ProcessUpdateRow(IMediaItem mediaItem, Action<IProcessRunner> completionCallback) :
        base(mediaItem.ParametersString, completionCallback)
    {
        Filepath = mediaItem.Filepath;
        Filename = FileSystem.GetFilenameWithoutExtension(Filepath);
        ViewItem = Ripper.CreateMediaViewItem(mediaItem, Tag);
        _viewCollection = new ViewCollection(ViewItem);
        AddToHistory(mediaItem);
        InitializeBuffer();
    }
    
    #endregion

    #region IProcessUpdateRow

    public async Task OpenInConsole()
    {
        await _console.Open(GetInstanceName());
    }
    
    public void Detach()
    {
        _console.Dispose();
    }

    #endregion
    
    #region Overrides

    private string? GetProcessStatus()
    {
        if (ProcessStatus is ProcessStatus.Running && !Started)
            return $"Process | {Messages.StartupTasks}";

        return GetStatus();
    }

    public override async Task<ProcessUpdateArgs> Update()
    {
        if (!(await base.Update()).Completed)
            return ProcessUpdateArgs.Default;

        if (GetProcessStatus() is not { } status || status.IsNullOrEmpty())
            return ProcessUpdateArgs.Default;
        
        SetViewField(() => UpdateViewItemFields(status));
        
        return ProcessUpdateArgs.Done;
    }

    public override async Task<bool> Start()
    {
        if (!await base.Start())
        {
            Stop();
            return false;
        }
        
        StartMessage();
        await RetrieveTitle();
        return true;
    }

    protected override async Task<bool> Complete()
    {
        if (!await base.Complete())
            return false;
        FinishMessage();
        return true;
    }

    protected override bool SetProcessStatus(ProcessStatus processStatus)
    {
        if (!base.SetProcessStatus(processStatus))
            return false;

        SetViewField(() =>
        {
            UpdateRowColors(processStatus);
            UpdateRowImage(processStatus);
            SetDefaultMessages(processStatus);
        });
        
        return true;
    }

    #endregion

    #region Protected Methods

    protected ProcessUpdateArgs UpdateViewItemFields(string status)
    {
        //ViewItem.Suspend();
        return new ProcessUpdateArgs(Completed, UpdateStatus(status));
        //ViewItem.Resume();
    }

    protected RowUpdateArgs? UpdateStatus(string statusMessage)
    {
        // TODO:
        //Status = statusMessage;

        if (Buffer.TokenizedProcessLine is not {Length: > 0} tokens)
            return default;
            
        // Download messages stream fast, bump the cursor up to one of the latest messages, if it exists...
        //  only start skipping cursor ahead once download messages have started
        //  otherwise important info could be skipped
        Buffer.SkipToEnd();

        return SetProgressText(tokens);
    }
    
    protected async Task RetrieveTitle()
    {
        if (Title.HasValue())
            return;
        
        SetTitle((await GetTitle()).ValueOrDefault(DefaultTitle));
    }

    #endregion

    #region Abstract Methods
    
    protected abstract Task<string> GetTitle();

    protected abstract RowUpdateArgs? SetProgressText(IReadOnlyList<string> tokens);

    protected abstract string? GetStatus();

    #endregion

    #region Private Methods

    private void SetTitle(string title)
    {
        SetViewField(() => Title = title);
    }

    private void AddToHistory(IMediaItem mediaItem)
    {
        History.Data.AddHistoryItem(Tag, mediaItem);
    }
    
    private void StartMessage()
    {
        History.Data.MarkStarted(Tag);
        Buffer.AddLog(Messages.ProcessStarted, ProcessLogType.Info);
        Output.WriteLine(string.Format(Messages.ProcessStartedTag, Tag));
    }

    private void FinishMessage()
    {
        History.Data.MarkCompleted(Tag, result:ProcessStatus);
        Buffer.AddLog(Messages.ProcessCompleted, ProcessLogType.Info);
        Output.WriteLine(string.Format(Messages.ProcessCompletedTag, Tag));
    }

    private void InitializeBuffer()
    {
        Buffer.LogAdded += OnLogAdded;
        Buffer.AddLog(Messages.ProcessInitialized, ProcessLogType.Info);
    }

    private string GetInstanceName()
    {
        string processName = FileSystem.GetFilename(ProcessFileName);
        string filename = FileSystem.GetFilename(Path);
        return processName.HasValue() && filename.HasValue() ? $"{processName} | {filename}" : string.Empty;
    }

    private void OnLogAdded(ProcessLogNode logNode)
    {
        _console.WriteOutput(logNode);
    }

    private void UpdateRowColors(ProcessStatus processStatus)
    {
        Color = processStatus switch
        {
            // Type Specific
            ProcessStatus.Running when this is DownloadProcessUpdateRow   => Color.Turquoise,
            ProcessStatus.Running when this is CompressProcessUpdateRow   => Color.MediumPurple,
            ProcessStatus.Running when this is ConversionProcessUpdateRow => Color.SaddleBrown,
            
            // General
            ProcessStatus.Queued    => Color.Bisque,
            ProcessStatus.Cancelled => Color.LightYellow,
            ProcessStatus.Succeeded => Color.LightGreen,
            ProcessStatus.Error     => Color.LightCoral,
            ProcessStatus.Stopped   => Color.DarkSalmon,
            ProcessStatus.Created   => Color.LightGray,
            ProcessStatus.Paused    => Color.DarkGray,
            _                       => Color.White
        };
    }

    private void UpdateRowImage(ProcessStatus processStatus)
    {
        ViewItem.ImageIndex = processStatus switch
        {
            // Type Specific
            ProcessStatus.Running when this is DownloadProcessUpdateRow => 3,
            ProcessStatus.Running when this is CompressProcessUpdateRow => 4,
            
            // General
            ProcessStatus.Queued    => 2,
            ProcessStatus.Cancelled => 8,
            ProcessStatus.Succeeded => 5,
            ProcessStatus.Error     => 6,
            ProcessStatus.Stopped   => 7,
            ProcessStatus.Paused    => 9,
            _                       => ViewItem.ImageIndex
        };
    }

    public void SetValues(ViewField fields, params string[] values)
    {
        Queue<string> valueQueue = new(values);
        if ((fields & ViewField.Status) > 0)
            Status = valueQueue.Dequeue();
        if ((fields & ViewField.Size) > 0)
            FileSize = valueQueue.Dequeue();
        if ((fields & ViewField.Progress) > 0)
            Progress = valueQueue.Dequeue();
        if ((fields & ViewField.Speed) > 0)
            Speed = valueQueue.Dequeue();
        if ((fields & ViewField.Eta) > 0)
            Eta = valueQueue.Dequeue();
    }

    private static readonly Dictionary<ProcessStatus, ViewField> _StatusToViewFieldsDict = new()
    {
        { ProcessStatus.Running,    ViewField.Status                                                        },
        { ProcessStatus.Queued,     ViewField.Status                                                        },
        { ProcessStatus.Created,    ViewField.Dynamic                                                       },
        { ProcessStatus.Succeeded,  ViewField.Status | ViewField.Progress | ViewField.Speed | ViewField.Eta },
        { ProcessStatus.Error,      ViewField.Status | ViewField.Size     | ViewField.Speed | ViewField.Eta },
        { ProcessStatus.Stopped,    ViewField.Status                      | ViewField.Speed | ViewField.Eta },
        { ProcessStatus.Cancelled,  ViewField.Dynamic                                                       },
        { ProcessStatus.Paused,     ViewField.Status                      | ViewField.Speed | ViewField.Eta }
    };
    
    private static readonly Dictionary<ProcessStatus, string> _StatusToMessageDict = new()
    {
        { ProcessStatus.Running,    Statuses.Starting  },
        { ProcessStatus.Queued,     Statuses.Queued    },
        { ProcessStatus.Created,    Statuses.Waiting   },
        { ProcessStatus.Succeeded,  Statuses.Succeeded },
        { ProcessStatus.Error,      Statuses.Error     },
        { ProcessStatus.Stopped,    Statuses.Stopped   },
        { ProcessStatus.Cancelled,  Statuses.Cancelled },
        { ProcessStatus.Paused,     Statuses.Paused    }
    };

    private void SetDefaultMessages(ProcessStatus processStatus)
    {
        ViewField flags = _StatusToViewFieldsDict[processStatus];
        string[] values;
        
        switch (processStatus)
        {
            default:
            case ProcessStatus.Running:
                values = new[]
                {
                    _StatusToMessageDict[processStatus]
                };
                break;
            case ProcessStatus.Queued:
                values = new[]
                {
                    _StatusToMessageDict[processStatus]
                };
                break;
            case ProcessStatus.Created:
                values = new[]
                {
                    _StatusToMessageDict[processStatus],
                    Text.DefaultSize,
                    Text.DefaultProgress,
                    Text.DefaultSpeed,
                    Text.DefaultTime
                };
                break;
            case ProcessStatus.Succeeded:
                values = new[]
                {
                    _StatusToMessageDict[processStatus],
                    Text.ProgressComplete,
                    Text.DefaultSpeed,
                    Text.DefaultTime
                };
                break;
            case ProcessStatus.Error:
                values = new[]
                {
                    _StatusToMessageDict[processStatus],
                    Text.DefaultSize,
                    Text.DefaultSpeed,
                    Text.DefaultTime
                };
                break;
            case ProcessStatus.Stopped:
                values = new[]
                {
                    _StatusToMessageDict[processStatus],
                    Text.DefaultSpeed,
                    Text.DefaultTime
                };
                break;
            case ProcessStatus.Cancelled:
                values = new[]
                {
                    _StatusToMessageDict[processStatus],
                    Text.DefaultSize,
                    Text.DefaultProgress,
                    Text.DefaultSpeed,
                    Text.DefaultTime
                };
                break;
            case ProcessStatus.Paused:
                values = new[]
                {
                    _StatusToMessageDict[processStatus],
                    Text.DefaultSpeed,
                    Text.DefaultTime
                };
                break;
        }

        SetValues(flags, values);
    }

    #endregion

    #region Static Methods

    // TODO: Currently this is causing deadlock on the main thread, unless we run in the background as so...
    protected static void SetViewField(Action setValueAction)
    {
        Task.Run(() => Threading.RunInMainContext(setValueAction));
    }

    #endregion
}