using JackTheVideoRipper.extensions;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models;
using JackTheVideoRipper.models.enums;

namespace JackTheVideoRipper
{
   public partial class FrameMain : Form
   {
      #region Data Members

      private IAsyncResult? _rowUpdateTask;

      private readonly ContextMenuManager _contextMenuManager;

      private readonly Ripper _ripper;

      #endregion

      #region Attributes

      private ListView.SelectedListViewItemCollection Selected => listItems.SelectedItems;

      private ListView.ListViewItemCollection ViewItems => listItems.Items;

      public IViewItem? FirstSelected => !NoneSelected ? Selected[0].As<IViewItem>() : null;

      public IViewItem? LastSelected => !NoneSelected ? Selected[^1].As<IViewItem>() : null;

      public bool NoneSelected => Selected.Count <= 0;

      public ListViewItem? FocusedItem => listItems.FocusedItem;

      public bool InItemBounds(MouseEventArgs e) => listItems.Visible && (FocusedItem?.InBounds(e.Location) ?? false);

      public string CachedSelectedTag { get; private set; } = string.Empty;

      private bool IsUpdating => _rowUpdateTask is not null && !_rowUpdateTask.IsCompleted;

      #endregion

      #region Events

      private event Action ManagerUpdated = delegate { };

      public event Action<object?, ContextActionEventArgs> ContextActionEvent = delegate { };

      public event Action<object?, DependencyActionEventArgs> DependencyActionEvent = delegate { };

      #endregion

      #region Form View Accessors

      private string NotificationStatus
      {
         set => notificationStatusLabel.Text = value;
      }

      private bool UpdateListViewRows
      {
         get => listItemRowsUpdateTimer.Enabled;
         set => listItemRowsUpdateTimer.Enabled = value;
      }

      private bool UpdateStatusBar
      {
         get => timerStatusBar.Enabled;
         set => timerStatusBar.Enabled = value;
      }

      private bool CheckForUpdates
      {
         get => timerCheckForUpdates.Enabled;
         set => timerCheckForUpdates.Enabled = value;
      }

      private bool UpdateProcessLimit
      {
         get => timerProcessLimit.Enabled;
         set => timerProcessLimit.Enabled = value;
      }

      #endregion

      #region Constructor

      public FrameMain(Ripper ripper)
      {
         // Needed for SubscribeEvents() calls (must come before)
         _ripper = ripper;

         InitializeComponent();

         SubscribeEvents();

         // Must come after InitializeComponents() call
         _contextMenuManager = new ContextMenuManager(contextMenuListItems);
      }

      #endregion

      #region Public Methods

      public void SetNotificationBrief(Notification notification)
      {
         string notificationMessage = notification.ShortenedMessage ?? notification.Message;

         Threading.RunInMainContext(() =>
         {
            NotificationStatus = $@"[{notification.DateQueued:T}]: {notificationMessage.TruncateEllipse(60)}";
         });
      }

      #endregion

      #region Private Methods

      private void InitializeViews()
      {
         Text = $@" {Core.ApplicationTitle}";

         listItems.OwnerDraw = true;
         listItems.DrawColumnHeader += DrawColumnHeader;
         listItems.DrawItem += DrawItem;

         OnSettingsUpdated(); //< Load initial values (for visibility bindings)
      }

      private readonly Font _headerFont = new("Segoe UI Semibold", 9.5f);

      private void DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
      {
         // Draw default background
         e.DrawBackground();

         // Draw text in a different font
         TextRenderer.DrawText(e.Graphics,
            e.Header!.Text,
            _headerFont,
            e.Bounds,
            SystemColors.ControlText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
      }

      private void DrawItem(object? sender, DrawListViewItemEventArgs e)
      {
         e.DrawDefault = true;
      }

      private async void Update(object? sender, EventArgs args)
      {
         if (!Core.IsConnectedToInternet())
            await _ripper.OnConnectionLost();

         if (IsUpdating)
            return;

         Application.DoEvents();

         _rowUpdateTask = UpdateModuleAsync(_ripper.Update);
      }

      private IAsyncResult? UpdateModuleAsync(Func<Task> updateModuleAction)
      {
         return Visible ? BeginInvoke(updateModuleAction, null) : default;
      }

      private void ClearAll()
      {
         ViewItems.Clear();
      }

      private void AddItem(IViewItem item)
      {
         if (item is not ListViewItem listViewItem)
            return;

         Threading.RunInMainContext(() => ViewItems.Add(listViewItem));
      }

      private void AddItems(IEnumerable<IViewItem> items)
      {
         if (items.Cast<ListViewItem>() is not { } listViewItems)
            return;

         Threading.RunInMainContext(() => ViewItems.AddRange(listViewItems));
      }

      private void RemoveItem(IViewItem item)
      {
         if (item is not ListViewItem listViewItem)
            return;

         Threading.RunInMainContext(() => ViewItems.Remove(listViewItem));
      }

      private void RemoveItems(IEnumerable<IViewItem> items)
      {
         if (items.Cast<ListViewItem>() is not { } listViewItems)
            return;

         Threading.RunInMainContext(() => ViewItems.RemoveRange(listViewItems));
      }

      private void StopUpdates()
      {
         // Cancel currently running update
         if (_rowUpdateTask is not null)
            EndInvoke(_rowUpdateTask);

         // Disable all timers
         CheckForUpdates = false;
         UpdateListViewRows = false;
         UpdateStatusBar = false;
         UpdateProcessLimit = false;
      }

      #endregion

      #region Timer Events

      private void InitializeTimers()
      {
         // Initiate the Update Loop
         UpdateListViewRows = true;
         UpdateStatusBar = true;
      }

      private async void TimerCheckForUpdates_Tick(object sender, EventArgs e)
      {
         CheckForUpdates = false;
         await Ripper.OnCheckForApplicationUpdates();
         CheckForUpdates = true;
      }

      private void TimerProcessLimit_Tick(object? sender = null, EventArgs? e = null)
      {
         UpdateProcessLimit = true;
      }

      #endregion

      #region Form Events

      private void OnFormLoad(object? sender, EventArgs e)
      {
         InitializeViews();
         Threading.InitializeScheduler();
         InitializeTimers();
      }

      private void OnFormShown(object? sender, EventArgs e)
      {

      }

      private void OnFormClosing(object? sender, FormClosingEventArgs e)
      {
         // Tells you if user cancelled
         _ripper.OnApplicationClosing(sender, e);
         if (e.Cancel)
            return;

         // Make sure our updates don't continue while we close, signal completion
         StopUpdates();
      }

      #endregion

      #region Event Handlers

      private async void KeyDownHandler(object? sender, KeyEventArgs args)
      {
         switch (args.KeyCode)
         {
            // Ctrl + V
            case Keys.V when args is { Control: true }:
               await _ripper.OnPasteContent();
               args.Handled = true;
               return;
            case Keys.Oemtilde:
               await Output.OpenMainConsoleWindow();
               args.Handled = true;
               break;
         }
      }

      private void OnUpdateStatusBar(object? sender, EventArgs args)
      {
         toolbarLabelStatus.Text = Statistics.Toolbar.ToolbarStatus;
         toolbarLabelProcessCounts.Text = Statistics.Toolbar.ToolbarProcessCount;
         toolBarLabelCpu.Text = Statistics.Toolbar.ToolbarCpu;
         toolBarLabelMemory.Text = Statistics.Toolbar.ToolbarMemory;
         toolBarLabelNetwork.Text = Statistics.Toolbar.ToolbarNetwork;
      }

      private void OnSettingsUpdated()
      {
         openConsoleToolStripMenuItem.Visible = Settings.Data.EnableDeveloperMode;
         openHistoryToolStripMenuItem.Visible = Settings.Data.StoreHistory;
      }

      private void OnClearNotifications()
      {
         Threading.RunInMainContext(() => NotificationStatus = string.Empty);
      }

      private void OnFormClick(object? sender, EventArgs args)
      {
         CachedSelectedTag = FirstSelected?.Tag ?? string.Empty;
      }

      private void OnDragEnter(object? sender, DragEventArgs e)
      {
         if (e.Data is null)
            return;

         e.Effect = e.IsValidDroppable() ?
             DragDropEffects.Copy :
             DragDropEffects.None;
      }

      private void OnDragDrop(object? sender, DragEventArgs e)
      {
         if (e.IsText() && e.AsText() is string content)
         {
            _ripper.OnDropUrl(content);
         }

         if (e.IsFile() && e.AsFile() is string[] { Length: > 0 } filepaths)
         {
            _ripper.OnDropFile(filepaths);
         }
      }

      private void SubscribeEvents()
      {
         // Bind to Settings Being Updated
         FrameSettings.SettingsUpdatedEvent += OnSettingsUpdated;

         // User Events
         SubscribeFormEvents();

         // Core Handlers
         SubscribeCoreHandlers();

         ManagerUpdated = delegate { TimerProcessLimit_Tick(); };
         _ripper.SubscribeMediaManagerEvents(ManagerUpdated, AddItem, AddItems,
             RemoveItem, RemoveItems);

         // Edit Menu
         SubscribeEditMenu();

         // Subpages
         SubscribeSubpageActions();

         // Core Buttons
         SubscribeCoreButtons();

         // Dependencies
         SubscribeDependencies();

         // Media Downloads
         SubscribeMediaTasks();

         // Tools
         SubscribeToolMenu();

         // Notifications
         SubscribeNotificationsBar();

         // Item Context Menu
         SubscribeContextEvents();
      }

      private void SubscribeCoreHandlers()
      {
         Load += OnFormLoad;
         Shown += OnFormShown;
         Shown += Ripper.OnEndStartup;
         FormClosing += OnFormClosing;
      }

      private void SubscribeFormEvents()
      {
         KeyDown += KeyDownHandler;
         Click += OnFormClick;
         listItems.Click += OnFormClick;
         contextMenuListItems.Click += OnFormClick;
         listItems.DragEnter += OnDragEnter;
         listItems.DragDrop += OnDragDrop;
         listItems.MouseClick += OnListItemsMouseClick;
      }

      private void SubscribeCoreButtons()
      {
         openDownloadFolderToolStripMenuItem.Click += Ripper.OnOpenDownloads;
         exitToolStripMenuItem.Click += (_, _) => Close();
         statusBar.DoubleClick += Ripper.OnOpenTaskManager;
         openTaskManagerToolStripMenuItem.Click += Ripper.OnOpenTaskManager;
         settingsToolStripMenuItem.Click += Ripper.OnOpenSettings;
         checkForUpdatesToolStripMenuItem.Click += Ripper.OnCheckForUpdates;
         openDependenciesFolderToolStripMenuItem.Click += Ripper.OnOpenInstallFolder;
      }

      private void SubscribeSubpageActions()
      {
         aboutToolStripMenuItem.Click += Ripper.OnOpenAbout;
         convertMediaToolStripMenuItem.Click += Ripper.OnOpenConvert;
      }

      private void SubscribeMediaTasks()
      {
         toolStripButtonDownloadVideo.Click += _ripper.OnDownloadVideo;
         toolStripButtonDownloadAudio.Click += _ripper.OnDownloadAudio;
         downloadVideoToolStripMenuItem.Click += _ripper.OnDownloadVideo;
         downloadAudioToolStripMenuItem.Click += _ripper.OnDownloadAudio;

         // Download Batch
         downloadBatchYouTubePlaylistlToolStripMenuItem.Click += _ripper.OnBatchPlaylist;
         downloadBatchDocumentToolStripMenuItem.Click += _ripper.OnBatchDocument;
         downloadBatchManualToolStripMenuItem.Click += _ripper.OnDownloadBatch;

         compressBatchToolStripMenuItem.Click += _ripper.OnCompressBulk;
      }

      private void SubscribeDependencies()
      {
         ytdlpToolStripMenuItem.Click += (sender, _) =>
             DependencyActionEvent(sender, new DependencyActionEventArgs(Dependencies.YouTubeDL));
         vS2010RedistributableToolStripMenuItem.Click += (sender, _) =>
             DependencyActionEvent(sender, new DependencyActionEventArgs(Dependencies.Redistributables));
         atomicParsleyToolStripMenuItem.Click += (sender, _) =>
             DependencyActionEvent(sender, new DependencyActionEventArgs(Dependencies.AtomicParsley));
         vlcPlayerToolStripMenuItem.Click += (sender, _) =>
             DependencyActionEvent(sender, new DependencyActionEventArgs(Dependencies.VLC));
         handbrakeToolStripMenuItem.Click += (sender, _) =>
             DependencyActionEvent(sender, new DependencyActionEventArgs(Dependencies.Handbrake));
         fFmpegToolStripMenuItem.Click += (sender, _) =>
             DependencyActionEvent(sender, new DependencyActionEventArgs(Dependencies.FFMPEG));
      }

      private void SubscribeToolMenu()
      {
         validateVideoToolStripMenuItem.Click += Ripper.OnVerifyIntegrity;
         compressVideoToolStripMenuItem.Click += _ripper.OnCompressVideo;
         repairVideoToolStripMenuItem.Click += _ripper.OnRepairVideo;
         recodeVideoToolStripMenuItem.Click += _ripper.OnRecodeVideo;
         openConsoleToolStripMenuItem.Click += Ripper.OnOpenConsole;
         openHistoryToolStripMenuItem.Click += Ripper.OnOpenHistory;
      }

      private void SubscribeEditMenu()
      {
         copyFailedUrlsToClipboardToolStripMenuItem.Click += _ripper.OnCopyFailedUrls;
         copyAllUrlsToClipboardToolStripMenuItem.Click += _ripper.OnCopyAllUrls;
         retryAllToolStripMenuItem.Click += _ripper.OnRetryAll;
         stopAllToolStripMenuItem.Click += _ripper.OnStopAll;
         clearFailuresToolStripMenuItem.Click += _ripper.OnRemoveFailed;
         clearAllToolStripMenuItem.Click += (_, _) => ClearAll();
         clearAllToolStripMenuItem.Click += _ripper.OnClearAllViewItems;
         clearSuccessesToolStripMenuItem.Click += _ripper.OnRemoveSucceeded;
         pauseAllToolStripMenuItem.Click += _ripper.OnPauseAll;
         resumeAllToolStripMenuItem.Click += _ripper.OnResumeAll;
      }

      private void SubscribeNotificationsBar()
      {
         NotificationsManager.SendNotificationEvent += SetNotificationBrief;
         NotificationsManager.ClearPushNotificationsEvent += OnClearNotifications;
         notificationStatusLabel.MouseDown += _ripper.OnNotificationBarClicked;
      }

      private async void OnListItemsMouseClick(object? sender, MouseEventArgs e)
      {
         if (e.IsRightClick() && InItemBounds(e))
            await _contextMenuManager.OpenContextMenu();
      }

      private void SubscribeContextEvents()
      {
         // File Options

         openFolderToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.Reveal));
         };

         openUrlInBrowserToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.OpenUrl));
         };

         openInMediaPlayerToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.OpenMedia));
         };

         openInConsoleToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.OpenConsole));
         };

         // Edit Options

         copyUrlToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.Copy));
         };

         copyCommandToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.CopyCommand));
         };

         // Process Options

         pauseProcessToolStripMenuItem.Click += (sender, _) =>
         {
            
         };

         resumeProcessToolStripMenuItem.Click += (sender, _) =>
         {
            
         };
         
         stopProcessToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.Stop));
         };
         
         retryProcessToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.Retry));
         };

         // Result Options

         deleteFromDiskToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.Delete));
         };
         
         reprocessMediaToolStripMenuItem.Click += (sender, _) =>
         {
            
         };
         
         // Miscellaneous

         removeRowToolStripMenuItem.Click += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.Remove));
         };

         listItems.DoubleClick += (sender, _) =>
         {
            ContextActionEvent(sender, new ContextActionEventArgs(ContextActions.OpenMedia));
         };
      }

      #endregion
   }
}
