using JackTheVideoRipper.extensions;
using JackTheVideoRipper.framework;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models;
using JackTheVideoRipper.models.processes;
using JackTheVideoRipper.models.rows;

namespace JackTheVideoRipper.views;

public partial class FrameMain : Form
{
   #region Data Members

   private IAsyncResult? _rowUpdateTask;

   private readonly ContextMenuManager _contextMenuManager;

   private readonly Ripper _ripper;

   #endregion

   #region Properties

   private ListView.SelectedListViewItemCollection Selected => listItems.SelectedItems;

   private ListView.ListViewItemCollection ViewItems => listItems.Items;

   public IViewItem? FirstSelected => !NoneSelected ? Selected[0].As<IViewItem>() : null;

   public IViewItem? LastSelected => !NoneSelected ? Selected[^1].As<IViewItem>() : null;

   public bool NoneSelected => Selected.Count <= 0;

   public ListViewItem? FocusedItem => listItems.FocusedItem;

   public bool InItemBounds(MouseEventArgs args) => listItems.Visible && (FocusedItem?.InBounds(args.Location) ?? false);

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
      UpdateViewElement(SetNotificationStatus);
      return;

      void SetNotificationStatus()
      {
         string notificationMessage = notification.ShortenedMessage ?? notification.Message;
         NotificationStatus = $@"[{notification.DateQueued:T}]: {notificationMessage.TruncateEllipse(60)}";
      }
   }

   #endregion

   #region Private Methods
      
   public static readonly AutoResetEvent UpdateViewHandle = new(true);
      
   private static void UpdateViewElement(Action action)
   {
      if (!Ripper.Visible)
         return;
         
      UpdateViewHandle.WaitOne(Global.Configurations.VIEW_UPDATE_TIMEOUT);
      Threading.RunInMainContext(action);
      UpdateViewHandle.Reset();
   }

   private static async Task UpdateViewElementAsync(Action action, CancellationToken? token = null)
   {
      if (!Ripper.Visible)
         return;
         
      UpdateViewHandle.WaitOne(Global.Configurations.VIEW_UPDATE_TIMEOUT);
      await Threading.RunInMainContext(action, token);
      UpdateViewHandle.Reset();
   }

   private void InitializeViews()
   {
      Text = $@" {Core.ApplicationTitle}";

      listItems.OwnerDraw = true;
      listItems.DrawColumnHeader += DrawColumnHeader;
      listItems.DrawItem += DrawItem;

      OnSettingsUpdated(); //< Load initial values (for visibility bindings)
   }
      
   private void UpdateView(IEnumerable<ProcessUpdateArgs> updateArgsEnumerable)
   {
      if (!Visible/* || !_ripper.WaitForNextDispatch(Global.Configurations.VIEW_UPDATE_TIMEOUT)*/)
         return;
         
      // TODO: Removed-- Causes Flickering
      //SuspendLayout();
         
      foreach (ProcessUpdateArgs args in updateArgsEnumerable)
      {
         if (args.Sender is not ProcessUpdateRow processUpdateRow || args.RowUpdateArgs is not { } updateArgs)
            continue;

         if (updateArgs.Progress is not null && processUpdateRow.CompareProgress(updateArgs.Progress) > 0)
            continue;
            
         //ViewItem? item = ViewItems.ToArray().FirstOrDefault(v => v.Tag == args.RowUpdateArgs.Tag) as ViewItem;

         SetFields(processUpdateRow, updateArgs);
      }
         
      // TODO: Removed-- Causes Flickering
      //ResumeLayout();
         
      Invalidate();
   }

   private static void SetFields(ProcessUpdateRow processUpdateRow, RowUpdateArgs updateArgs)
   {
      ViewField flags = 0;
      List<string> values = new(8);

      if (updateArgs.Status is not null)
      {
         flags |= ViewField.Status;
         values.Add(updateArgs.Status);
      }

      if (updateArgs.MediaType is not null)
      {
         flags |= ViewField.MediaType;
         values.Add(updateArgs.MediaType);
      }

      if (updateArgs.FileSize is not null)
      {
         flags |= ViewField.Size;
         values.Add(updateArgs.FileSize);
      }

      if (updateArgs.Progress is not null)
      {
         flags |= ViewField.Progress;
         values.Add(updateArgs.Progress);
      }

      if (updateArgs.Speed is not null)
      {
         flags |= ViewField.Speed;
         values.Add(updateArgs.Speed);
      }

      if (updateArgs.Eta is not null)
      {
         flags |= ViewField.Eta;
         values.Add(updateArgs.Eta);
      }

      if (updateArgs.Url is not null)
      {
         flags |= ViewField.Url;
         values.Add(updateArgs.Url);
      }

      if (updateArgs.Path is not null)
      {
         flags |= ViewField.Path;
         values.Add(updateArgs.Path);
      }
         
      if (flags > 0)
         UpdateViewElement(() => processUpdateRow.SetValues(flags, values.ToArray()));
   }

   private static void DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs args)
   {
      // Draw default background
      args.DrawBackground();

      // Draw text in a different font
      TextRenderer.DrawText(args.Graphics, args.Header?.Text, Global.Fonts.Header, args.Bounds,
         SystemColors.ControlText, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
   }

   private static void DrawItem(object? sender, DrawListViewItemEventArgs args)
   {
      args.DrawDefault = true;
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

      UpdateViewElement(() => ViewItems.Add(listViewItem));
   }

   private void AddItems(IViewItemEnumerable items)
   {
      if (items.Cast<ListViewItem>() is not { } listViewItems)
         return;

      UpdateViewElement(() => ViewItems.AddRange(listViewItems));
   }

   private void RemoveItem(IViewItem item)
   {
      if (item is not ListViewItem listViewItem)
         return;

      UpdateViewElement(() => ViewItems.Remove(listViewItem));
   }

   private void RemoveItems(IViewItemEnumerable items)
   {
      if (items.Cast<ListViewItem>() is not { } listViewItems)
         return;

      UpdateViewElement(() => ViewItems.RemoveRange(listViewItems));
   }

   private void StopUpdates()
   {
      // Cancel currently running update
      if (_rowUpdateTask is not null)
         EndInvoke(_rowUpdateTask);

      // Disable all timers
      CheckForUpdates      = false;
      UpdateListViewRows   = false;
      UpdateStatusBar      = false;
      UpdateProcessLimit   = false;
   }

   #endregion
}