namespace JackTheVideoRipper.views
{
   partial class FrameConsole
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         components = new System.ComponentModel.Container();
         ConsoleControl = new ConsoleControl.ConsoleControl();
         consoleContextMenu = new ContextMenuStrip(components);
         saveToFileToolStripMenuItem = new ToolStripMenuItem();
         consoleContextMenu.SuspendLayout();
         SuspendLayout();
         // 
         // ConsoleControl
         // 
         ConsoleControl.AutoSize = true;
         ConsoleControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
         ConsoleControl.AutoValidate = AutoValidate.EnableAllowFocusChange;
         ConsoleControl.ContextMenuStrip = consoleContextMenu;
         ConsoleControl.Dock = DockStyle.Fill;
         ConsoleControl.IsInputEnabled = true;
         ConsoleControl.Location = new Point(0, 0);
         ConsoleControl.Margin = new Padding(5, 4, 5, 4);
         ConsoleControl.Name = "ConsoleControl";
         ConsoleControl.Padding = new Padding(10, 11, 10, 11);
         ConsoleControl.SendKeyboardCommandsToProcess = false;
         ConsoleControl.ShowDiagnostics = false;
         ConsoleControl.Size = new Size(914, 600);
         ConsoleControl.TabIndex = 0;
         // 
         // consoleContextMenu
         // 
         consoleContextMenu.ImageScalingSize = new Size(20, 20);
         consoleContextMenu.Items.AddRange(new ToolStripItem[] { saveToFileToolStripMenuItem });
         consoleContextMenu.Name = "Console";
         consoleContextMenu.Size = new Size(157, 28);
         // 
         // saveToFileToolStripMenuItem
         // 
         saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
         saveToFileToolStripMenuItem.Size = new Size(156, 24);
         saveToFileToolStripMenuItem.Text = "Save To File";
         // 
         // FrameConsole
         // 
         AutoScaleDimensions = new SizeF(8F, 20F);
         AutoScaleMode = AutoScaleMode.Font;
         AutoSizeMode = AutoSizeMode.GrowAndShrink;
         AutoValidate = AutoValidate.EnableAllowFocusChange;
         ClientSize = new Size(914, 600);
         Controls.Add(ConsoleControl);
         DoubleBuffered = true;
         KeyPreview = true;
         Margin = new Padding(3, 4, 3, 4);
         Name = "FrameConsole";
         ShowIcon = false;
         StartPosition = FormStartPosition.CenterParent;
         Text = "Console";
         TransparencyKey = Color.Transparent;
         consoleContextMenu.ResumeLayout(false);
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      public ConsoleControl.ConsoleControl ConsoleControl;
      private ContextMenuStrip consoleContextMenu;
      private ToolStripMenuItem saveToFileToolStripMenuItem;
   }
}