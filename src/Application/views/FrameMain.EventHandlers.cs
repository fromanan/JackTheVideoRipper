using JackTheVideoRipper.extensions;
using JackTheVideoRipper.framework;
using JackTheVideoRipper.models;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.viewmodels;

namespace JackTheVideoRipper.views;

public partial class FrameMain
{
    #region Timer Events

   private void InitializeTimers()
   {
      // Initiate the Update Loop
      UpdateListViewRows   = true;
      UpdateStatusBar      = true;
   }

   private async void TimerCheckForUpdates_Tick(object sender, EventArgs args)
   {
      CheckForUpdates = false;
      await Ripper.OnCheckForApplicationUpdates();
      CheckForUpdates = true;
   }

   private void TimerProcessLimit_Tick(object? sender = null, EventArgs? args = null)
   {
      UpdateProcessLimit = true;
   }

   #endregion

   #region Form Events

   private void OnFormLoad(object? sender, EventArgs args)
   {
      InitializeViews();
      Threading.InitializeScheduler();
      InitializeTimers();
   }

   private void OnFormShown(object? sender, EventArgs args)
   {

   }

   private void OnFormClosing(object? sender, FormClosingEventArgs args)
   {
      // Tells you if user cancelled
      _ripper.OnApplicationClosing(sender, args);
      if (args.Cancel)
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
         // Ctrl + C -- Copy
         case Keys.C when args is { Control: true }:
            ContextAction(sender, ContextActions.CopyUrl);
            args.Handled = true;
            return;
         // Ctrl + V -- Paste
         case Keys.V when args is { Control: true }:
            await _ripper.OnPasteContent();
            args.Handled = true;
            return;
         // Ctrl + R -- Refresh
         case Keys.R when args is { Control: true }:
            await _ripper.OnRefresh();
            args.Handled = true;
            return;
         // Ctrl + O -- Open
         case Keys.O when args is { Control: true }:
            ContextAction(sender, ContextActions.OpenMedia);
            return;
         // Ctrl + E -- Explorer
         case Keys.E when args is { Control: true }:
            ContextAction(sender, ContextActions.Reveal);
            return;
         case Keys.Oemtilde:
            await Output.OpenMainConsoleWindow();
            args.Handled = true;
            return;
         case Keys.Delete:
            ContextAction(sender, ContextActions.Remove);
            args.Handled = true;
            return;
      }
   }

   private void OnUpdateStatusBar(object? sender, EventArgs args)
   {
      toolbarLabelStatus.Text          = Statistics.Toolbar.ToolbarStatus;
      toolbarLabelProcessCounts.Text   = Statistics.Toolbar.ToolbarProcessCount;
      toolBarLabelCpu.Text             = Statistics.Toolbar.ToolbarCpu;
      toolBarLabelMemory.Text          = Statistics.Toolbar.ToolbarMemory;
      toolBarLabelNetwork.Text         = Statistics.Toolbar.ToolbarNetwork;
   }

   private void OnSettingsUpdated()
   {
      openConsoleToolStripMenuItem.Visible = Settings.Data.EnableDeveloperMode;
      openHistoryToolStripMenuItem.Visible = Settings.Data.StoreHistory;
   }

   private void OnClearNotifications()
   {
      UpdateViewElement(() => NotificationStatus = string.Empty);
   }

   private void OnFormClick(object? sender, EventArgs args)
   {
      CachedSelectedTag = FirstSelected?.Tag ?? string.Empty;
   }

   private void OnDragEnter(object? sender, DragEventArgs args)
   {
      if (args.Data is null)
         return;

      args.Effect = args.IsValidDroppable() ?
         DragDropEffects.Copy :
         DragDropEffects.None;
   }

   private void OnDragDrop(object? sender, DragEventArgs args)
   {
      if (args.IsText() && args.AsText() is string content)
      {
         _ripper.OnDropUrl(content);
      }

      if (args.IsFile() && args.AsFile() is string[] { Length: > 0 } filepaths)
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
         RemoveItem, RemoveItems, UpdateView);

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
      Load                                                  += OnFormLoad;
      Shown                                                 += OnFormShown;
      Shown                                                 += Ripper.OnEndStartup;
      FormClosing                                           += OnFormClosing;
   }

   private void SubscribeFormEvents()
   {
      KeyDown                                               += KeyDownHandler;
      Click                                                 += OnFormClick;
      listItems.Click                                       += OnFormClick;
      contextMenuListItems.Click                            += OnFormClick;
      listItems.DragEnter                                   += OnDragEnter;
      listItems.DragDrop                                    += OnDragDrop;
      listItems.MouseClick                                  += OnListItemsMouseClick;
   }

   private void SubscribeCoreButtons()
   {
      openDownloadFolderToolStripMenuItem.Click             += Ripper.OnOpenDownloads;
      exitToolStripMenuItem.Click                           += (_, _) => Close();
      statusBar.DoubleClick                                 += Ripper.OnOpenTaskManager;
      openTaskManagerToolStripMenuItem.Click                += Ripper.OnOpenTaskManager;
      settingsToolStripMenuItem.Click                       += Ripper.OnOpenSettings;
      checkForUpdatesToolStripMenuItem.Click                += Ripper.OnCheckForUpdates;
      openDependenciesFolderToolStripMenuItem.Click         += Ripper.OnOpenInstallFolder;
   }

   private void SubscribeSubpageActions()
   {
      aboutToolStripMenuItem.Click                          += Ripper.OnOpenAbout;
      convertToolStripMenuItem.Click                        += Ripper.OnOpenConvert;
   }

   private void SubscribeMediaTasks()
   {
      toolStripButtonDownloadVideo.Click                    += _ripper.OnDownloadVideo;
      toolStripButtonDownloadAudio.Click                    += _ripper.OnDownloadAudio;
      downloadVideoToolStripMenuItem.Click                  += _ripper.OnDownloadVideo;
      downloadAudioToolStripMenuItem.Click                  += _ripper.OnDownloadAudio;

      // Download Batch
      downloadBatchYouTubePlaylistlToolStripMenuItem.Click  += _ripper.OnBatchPlaylist;
      downloadBatchDocumentToolStripMenuItem.Click          += _ripper.OnBatchDocument;
      downloadBatchManualToolStripMenuItem.Click            += _ripper.OnDownloadBatch;
         
      // Other Batch Options
      compressBatchToolStripMenuItem.Click                  += _ripper.OnCompressBulk;
   }
      
   private void DependencyAction(object? sender, Dependencies dependency)
   {
      DependencyActionEvent(sender, new DependencyActionEventArgs(dependency));
   }

   private void SubscribeDependencies()
   {
      ytdlpToolStripMenuItem.Click += (sender, _) =>
         DependencyAction(sender, Dependencies.YouTubeDL);
      vS2010RedistributableToolStripMenuItem.Click += (sender, _) =>
         DependencyAction(sender, Dependencies.Redistributables);
      atomicParsleyToolStripMenuItem.Click += (sender, _) =>
         DependencyAction(sender, Dependencies.AtomicParsley);
      vlcPlayerToolStripMenuItem.Click += (sender, _) =>
         DependencyAction(sender, Dependencies.VLC);
      handbrakeToolStripMenuItem.Click += (sender, _) =>
         DependencyAction(sender, Dependencies.Handbrake);
      fFmpegToolStripMenuItem.Click += (sender, _) =>
         DependencyAction(sender, Dependencies.FFMPEG);
   }

   private void SubscribeToolMenu()
   {
      validateVideoToolStripMenuItem.Click                  += Ripper.OnVerifyIntegrity;
      compressVideoToolStripMenuItem.Click                  += _ripper.OnCompressVideo;
      repairVideoToolStripMenuItem.Click                    += _ripper.OnRepairVideo;
      recodeVideoToolStripMenuItem.Click                    += _ripper.OnRecodeVideo;
      openConsoleToolStripMenuItem.Click                    += Ripper.OnOpenConsole;
      openHistoryToolStripMenuItem.Click                    += Ripper.OnOpenHistory;
   }

   private void SubscribeEditMenu()
   {
      copyFailedUrlsToClipboardToolStripMenuItem.Click      += _ripper.OnCopyFailedUrls;
      copyAllUrlsToClipboardToolStripMenuItem.Click         += _ripper.OnCopyAllUrls;
      retryAllToolStripMenuItem.Click                       += _ripper.OnRetryAll;
      stopAllToolStripMenuItem.Click                        += _ripper.OnStopAll;
      clearFailuresToolStripMenuItem.Click                  += _ripper.OnRemoveFailed;
      clearAllToolStripMenuItem.Click                       += (_, _) => ClearAll();
      clearAllToolStripMenuItem.Click                       += _ripper.OnClearAllViewItems;
      clearSuccessesToolStripMenuItem.Click                 += _ripper.OnRemoveSucceeded;
      pauseAllToolStripMenuItem.Click                       += _ripper.OnPauseAll;
      resumeAllToolStripMenuItem.Click                      += _ripper.OnResumeAll;
   }

   private void SubscribeNotificationsBar()
   {
      NotificationsManager.SendNotificationEvent            += SetNotificationBrief;
      NotificationsManager.ClearPushNotificationsEvent      += OnClearNotifications;
      notificationStatusLabel.MouseDown                     += _ripper.OnNotificationBarClicked;
   }

   private async void OnListItemsMouseClick(object? sender, MouseEventArgs args)
   {
      if (args.IsRightClick() && InItemBounds(args))
         await _contextMenuManager.OpenContextMenu();
   }

   private void ContextAction(object? sender, ContextActions contextAction)
   {
      ContextActionEvent(sender, new ContextActionEventArgs(contextAction));
   }

   private void SubscribeContextEvents()
   {
      // File Options

      openFolderToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Reveal);
      };

      openUrlInBrowserToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.OpenUrl);
      };

      openInMediaPlayerToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.OpenMedia);
      };

      openInConsoleToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.OpenConsole);
      };

      // Edit Options

      copyUrlToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.CopyUrl);
      };

      copyCommandToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.CopyCommand);
      };

      // Process Options

      pauseProcessToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Pause);
      };

      resumeProcessToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Resume);
      };

      stopProcessToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Stop);
      };

      retryProcessToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Retry);
      };

      // Result Options

      deleteFromDiskToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Delete);
      };

      reprocessMediaToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Reprocess);
      };
         
      convertToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Convert);
      };

      compressToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Compress);
      };

      // Miscellaneous

      removeRowToolStripMenuItem.Click += (sender, _) =>
      {
         ContextAction(sender, ContextActions.Remove);
      };

      listItems.DoubleClick += (sender, _) =>
      {
         ContextAction(sender, ContextActions.OpenMedia);
      };
   }

   #endregion
}