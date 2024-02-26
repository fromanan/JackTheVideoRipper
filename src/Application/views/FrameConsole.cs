using JackTheVideoRipper.extensions;
using JackTheVideoRipper.framework;

namespace JackTheVideoRipper.views
{
   public partial class FrameConsole : Form
   {
      #region Data Members

      private readonly string _instanceName;

      private static readonly Font _DefaultFont = new("Lucinda Console", 11);

      public bool Frozen { get; private set; }

      public event Action FreezeConsoleEvent = delegate { };

      public event Action UnfreezeConsoleEvent = delegate { };

      private bool _suspended;

      #endregion

      #region Properties

      public bool InItemBounds(MouseEventArgs args) => ConsoleControl.Bounds.Contains(args.Location);

      private RichTextBox TextBox => ConsoleControl.InternalRichTextBox;

      #endregion

      #region Constructor

      public FrameConsole(string instanceName)
      {
         _instanceName = instanceName;
         InitializeComponent();
         SubscribeEvents();
      }

      #endregion

      #region Public Methods

      public async Task OpenConsole()
      {
         await Threading.RunInMainContext(Show);
         await MoveToTop();
      }

      #endregion

      #region Private Methods

      public async Task MoveToTop()
      {
         await Threading.RunInMainContext(Activate);
      }

      private void ShowContextMenu()
      {
         consoleContextMenu.Show(Cursor.Position);
      }

      #endregion

      #region Overrides

      protected override void OnFormClosing(FormClosingEventArgs args)
      {
         base.OnFormClosing(args);

         if (args.CloseReason == CloseReason.WindowsShutDown)
            return;

         Visible = false;
         args.Cancel = true;
      }

      #endregion

      #region Event Handlers

      private void SubscribeEvents()
      {
         Load += FrameConsole_Load;
         KeyDown += OnKeyPress;
         TextBox.VScroll += UpdateConsoleFrozen;
         TextBox.MouseDown += OnMouseDown;
         TextBox.SelectionChanged += UpdateConsoleFrozen;
         TextBox.LostFocus += OnLostFocus;
         TextBox.GotFocus += OnGotFocus;
         saveToFileToolStripMenuItem.Click += OnSaveToFile;
      }

      private void OnLostFocus(object? sender, EventArgs args)
      {
         TextBox.Suspend();
         _suspended = true;
         UpdateConsoleFrozen(sender, args);
      }

      private void OnGotFocus(object? sender, EventArgs args)
      {
         TextBox.Resume();
         _suspended = false;
         UpdateConsoleFrozen(sender, args);
      }

      private void FrameConsole_Load(object? sender, EventArgs args)
      {
         Text = _instanceName.HasValue() ? $"Console | {_instanceName}" : "Console";
         AllowTransparency = false;
         TextBox.Font = _DefaultFont;
      }

      private void OnSaveToFile(object? sender, EventArgs args)
      {
         if (FileSystem.SaveFileUsingDialog() is not { } filename || filename.IsNullOrEmpty())
            return;

         // TODO: Use the results from the process?
         FileSystem.SaveToFile(filename, TextBox.Text);
      }

      private void OnMouseDown(object? sender, MouseEventArgs args)
      {
         if (args.IsRightClick() && InItemBounds(args))
            ShowContextMenu();
      }

      private async void UpdateConsoleFrozen(object? sender, EventArgs args)
      {
         if (_suspended || TextBox.IsAtMaxScroll() && !TextBox.HasSelected())
         {
            if (Frozen)
               await Tasks.StartAfter(UnfreezeConsole);
         }
         else if (!Frozen)
         {
            FreezeConsole();
         }
      }

      private void FreezeConsole()
      {
         Frozen = true;
         FreezeConsoleEvent();
      }

      private void UnfreezeConsole()
      {
         Frozen = false;
         UnfreezeConsoleEvent();
      }

      private void OnKeyPress(object? sender, KeyEventArgs args)
      {
         switch (args.KeyCode)
         {
            // Ctrl + A
            case Keys.A when args is { Control: true }:
               TextBox.SelectAll();
               args.Handled = true;
               return;
            // Ctrl + D
            case Keys.D when args is { Control: true }:
               TextBox.DeselectAll();
               args.Handled = true;
               return;
            case Keys.Delete:
               ConsoleControl.ClearOutput();
               args.Handled = true;
               return;
            case Keys.Oemtilde:
               Close();
               args.Handled = true;
               return;
         }
      }

      #endregion
   }
}
