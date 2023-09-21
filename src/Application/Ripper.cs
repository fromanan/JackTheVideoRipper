using JackTheVideoRipper.extensions;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.modules;
using JackTheVideoRipper.views;
using static JackTheVideoRipper.FileSystem;

namespace JackTheVideoRipper;

/**
 * Serves as interface between view and model
 */
public class Ripper
{
    public static Ripper Instance = null!;
    
    #region Data Members

    private readonly MediaManager _mediaManager = new();
    private readonly NotificationsManager _notificationsManager = new();
    public readonly FrameMain FrameMain;

    private readonly IViewItemProvider _viewItemProvider;

    #endregion

    #region Attributes

    public static string SelectedTag => Instance.FrameMain.CachedSelectedTag;
    
    public static IViewItemProvider ViewItemProvider => Instance._viewItemProvider;

    #endregion

    #region ViewItem Provider Functions

    public static IViewItem CreateViewItem(string? tag = null)
    {
        return ViewItemProvider.CreateViewItem(tag);
    }
    
    public static IViewItem CreateViewItem(string[] items, string? tag = null)
    {
        return ViewItemProvider.CreateViewItem(items, tag);
    }
    
    public static IViewItem CreateMediaViewItem(IMediaItem mediaItem, string? tag = null)
    {
        return ViewItemProvider.CreateMediaViewItem(mediaItem, tag);
    }

    #endregion

    #region Constructor

    public Ripper(IViewItemProvider viewItemProvider)
    {
        _viewItemProvider = viewItemProvider;
        FrameMain = new FrameMain(this);
        SubscribeEvents();
    }

    #endregion

    #region Public Methods

    public string GetProgramStatus()
    {
        return _mediaManager.GetStatus();
    }

    public int[] GetProcessCounts()
    {
        return _mediaManager.GetProcessCounts();
    }

    public ProcessStatus? GetSelectedStatus()
    {
        return Instance.GetStatus(FrameMain.CachedSelectedTag);
    }
    
    public Type? GetSelectedProcessType()
    {
        return _mediaManager.GetRow(FrameMain.CachedSelectedTag)?.GetType();
    }

    public bool SelectedIsType<T>()
    {
        return GetSelectedProcessType() == typeof(T);
    }

    public ProcessStatus? GetStatus(string tag)
    {
        return _mediaManager.GetRow(tag)?.ProcessStatus;
    }

    public async Task Update()
    {
        await _mediaManager.UpdateListItemRows();
    }

    #endregion

    #region Event Handlers
    
    private void SubscribeEvents()
    {
        NotificationsManager.SendNotificationEvent += _ => _notificationsManager.Reset();
        FrameMain.ContextActionEvent += OnContextAction;
        FrameMain.DependencyActionEvent += OnUpdateDependency;
        FrameMain.Shown += (_, _) => _notificationsManager.Start();
    }

    public void SubscribeMediaManagerEvents(Action updateEventHandler,
        Action<IViewItem> addAction,
        Action<IEnumerable<IViewItem>> addMultiAction,
        Action<IViewItem> removeAction,
        Action<IEnumerable<IViewItem>> removeMultiAction)
    {
        _mediaManager.QueueUpdated += updateEventHandler;
        _mediaManager.ProcessAdded += addAction;
        _mediaManager.ProcessesAdded += addMultiAction;
        _mediaManager.ProcessRemoved += removeAction;
        _mediaManager.ProcessesRemoved += removeMultiAction;
    }

    public async void OnDropUrl(string content)
    {
        if (content.Invalid(IsValidUrl))
            return;

        await _mediaManager.DownloadFromUrl(content);
    }
    
    // TODO: Handle folders, handle audio files (or potentially images)
    public async void OnDropFile(string[] filepaths)
    {
        if (filepaths.Length == 0)
            return;
        
        FFMPEG.Operation[] options =
        {
            FFMPEG.Operation.Compress,
            FFMPEG.Operation.Repair,
            FFMPEG.Operation.Recode,
            //FFMPEG.Operation.Convert,
            //FFMPEG.Operation.AddAudio,
            //FFMPEG.Operation.Validate
        };

        string? selection = Modals.BasicDropdown(options.Select(o => o.ToString()), "Operation Select");
        if (selection is null || !Enum.TryParse(selection, out FFMPEG.Operation operation))
            return;
        
        // Single file operation
        if (filepaths.Length == 1)
        {
            string filepath = filepaths[0];
            if (filepath.Invalid(IsValidPath) || !Formats.IsVideoFormat(filepath))
                return;

            switch (operation)
            {
                case FFMPEG.Operation.Compress:
                    await _mediaManager.CompressVideo(filepath);
                    return;
                case FFMPEG.Operation.Repair:
                    await _mediaManager.RepairVideo(filepath);
                    return;
                case FFMPEG.Operation.Recode:
                    await _mediaManager.RecodeVideo(filepath);
                    return;
                case FFMPEG.Operation.Convert:
                    await _mediaManager.ConvertVideo(filepath);
                    return;
                case FFMPEG.Operation.AddAudio:
                    await _mediaManager.AddAudio(filepath);
                    return;
                case FFMPEG.Operation.Validate:
                    await _mediaManager.ValidateVideo(filepath);
                    return;
            }
        }
        else // Batch operation
        {
            IEnumerable<string> validPaths = filepaths.Where(f => f.Valid(IsValidPath) && Formats.IsVideoFormat(f));
            switch (operation)
            {
                case FFMPEG.Operation.Compress:
                    await _mediaManager.CompressBulk(validPaths);
                    return;
                case FFMPEG.Operation.Repair:
                    throw new NotImplementedException();
                    return;
                case FFMPEG.Operation.Recode:
                    throw new NotImplementedException();
                    return;
                case FFMPEG.Operation.Convert:
                    throw new NotImplementedException();
                    return;
                case FFMPEG.Operation.AddAudio:
                    throw new NotImplementedException();
                    return;
                case FFMPEG.Operation.Validate:
                    throw new NotImplementedException();
                    return;
            }
        }
    }

    public async Task OnPasteContent()
    {
        string url = GetClipboardText();
            
        if (url.Invalid(IsValidUrl))
            return;

        await _mediaManager.DownloadFromUrl(url);
    }

    public async Task OnRefresh()
    {
        await _mediaManager.Refresh();
    }

    public async void OnCompressVideo(object? sender, EventArgs e)
    {
        if (SelectFile() is not { } filepath)
            return;
             
        await _mediaManager.CompressVideo(filepath);
    }
        
    public async void OnCompressBulk(object? sender, EventArgs e)
    {
        if (SelectFolder() is not { } directoryPath)
            return;

        await _mediaManager.CompressBulk(directoryPath);
    }
        
    public async void OnRecodeVideo(object? sender, EventArgs e)
    {
        if (SelectFile() is not { } filepath)
            return;

        await _mediaManager.RecodeVideo(filepath);
    }
        
    public async void OnRepairVideo(object? sender, EventArgs e)
    {
        if (SelectFile() is not { } filepath)
            return;

        await _mediaManager.RepairVideo(filepath);
    }
    
    public async void OnDownloadVideo(object? sender, EventArgs e)
    {
        await _mediaManager.DownloadMediaDialog(MediaType.Video);
    }
    
    public async void OnDownloadAudio(object? sender, EventArgs e)
    {
        await _mediaManager.DownloadMediaDialog(MediaType.Audio);
    }
    
    // TODO: Should it just not wait and notify user of reconnection?
    public async Task OnConnectionLost()
    {
        Modals.Notification(Messages.LostConnection);
        _mediaManager.PauseAll();
        await Tasks.WaitUntil(Core.IsConnectedToInternet); //< Delay processes until connected
        _mediaManager.ResumeAll();
    }

    public async void OnContextAction(object? sender, ContextActionEventArgs e)
    {
        await _mediaManager.PerformContextAction(e.ContextAction);
    }

    public void OnNotificationBarClicked(object? sender, MouseEventArgs e)
    {
        if (e.IsRightClick())
        {
            NotificationsManager.ClearPushNotifications();
        }
        else
        {
            _notificationsManager.OpenNotificationWindow();
        }
    }

    public async void OnApplicationClosing(object? sender, FormClosingEventArgs e)
    {
        if (_mediaManager.OnFormClosing())
        {
            e.Cancel = true;
            return;
        }
        
        Output.SaveLogs();
        
        // Allow normal shutdown for all subscribers
        await Core.Shutdown();
    }
    
    // Edit Menu
    public void OnCopyFailedUrls(object? sender, EventArgs e)
    {
        _mediaManager.CopyFailedUrls();
    }
    
    public void OnCopyAllUrls(object? sender, EventArgs e)
    {
        _mediaManager.CopyAllUrls();
    }

    public void OnRetryAll(object? sender, EventArgs e)
    {
        _mediaManager.RetryAll();
    }

    public void OnStopAll(object? sender, EventArgs e)
    {
        _mediaManager.StopAll();
    }

    public void OnRemoveFailed(object? sender, EventArgs e)
    {
        _mediaManager.RemoveFailed();
    }

    public void OnClearAll(object? sender, EventArgs e)
    {
        _mediaManager.ClearAll();
    }

    public void OnRemoveSucceeded(object? sender, EventArgs e)
    {
        _mediaManager.RemoveSucceeded();
    }

    public void OnPauseAll(object? sender, EventArgs e)
    {
        _mediaManager.PauseAll();
    }

    public void OnResumeAll(object? sender, EventArgs e)
    {
        _mediaManager.ResumeAll();
    }
    
    public async void OnBatchPlaylist(object? sender, EventArgs e)
    {
        await _mediaManager.BatchPlaylist();
    }

    public async void OnBatchDocument(object? sender, EventArgs e)
    {
        await _mediaManager.BatchDocument();
    }

    public async void OnDownloadBatch(object? sender, EventArgs e)
    {
        await _mediaManager.DownloadBatch();
    }

    public static void OnVerifyIntegrity(object? sender, EventArgs e)
    {
        MediaManager.VerifyIntegrity();
    }

    public static async void OnOpenConsole(object? sender, EventArgs e)
    {
        await Output.OpenMainConsoleWindow();
    }

    public static void OnOpenHistory(object? sender, EventArgs e)
    {
        Pages.OpenPageBackground<FrameHistory>();
    }
    
    public static async void OnUpdateDependency(object? sender, DependencyActionEventArgs e)
    {
        await Core.UpdateDependency(e.Dependency);
    }

    public static void OnEndStartup(object? sender, EventArgs e)
    {
        Statistics.EndStartup();
        Output.WriteLine(Statistics.StartupMessage, sendAsNotification:true);
    }
    
    public void OnClearAllViewItems(object? sender, EventArgs e)
    {
        _mediaManager.ClearAll();
    }

    public static void OnOpenDownloads(object? sender, EventArgs e)
    {
        Task.Run(OpenDownloads);
    }

    public static void OnOpenTaskManager(object? sender, EventArgs e)
    {
        Task.Run(OpenTaskManager);
    }

    public static void OnCheckForUpdates(object? sender, EventArgs e)
    {
        Task.Run(Core.CheckForUpdates);
    }

    public static void OnOpenSettings(object? sender, EventArgs e)
    {
        Pages.OpenPage<FrameSettings>();
    }

    public static void OnOpenInstallFolder(object? sender, EventArgs e)
    {
        Common.OpenInstallFolder();
    }

    public static void OnOpenAbout(object? sender, EventArgs e)
    {
        Pages.OpenPage<FrameAbout>();
    }

    public static void OnOpenConvert(object? sender, EventArgs e)
    {
        Pages.OpenPage<FrameConvert>();
    }

    public static async Task OnCheckForApplicationUpdates()
    {
        await AppUpdate.CheckForNewAppVersion(false);
    }
    
    #endregion
}