﻿using System.ComponentModel;
using System.Diagnostics;
using ConsoleControlAPI;

namespace ConsoleControl
{
    /// <summary>
    /// The console event handler is used for console events.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ConsoleEventArgs"/> instance containing the event data.</param>
    public delegate void ConsoleEventHandler(object sender, ConsoleEventArgs args);

    /// <summary>
    /// The Console Control allows you to embed a basic console in your application.
    /// </summary>
    [ToolboxBitmap(typeof(Resfinder), "ConsoleControl.ConsoleControl.bmp")]
    public partial class ConsoleControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleControl"/> class.
        /// </summary>
        public ConsoleControl()
        {
            //  Initialise the component.
            InitializeComponent();

            //  Show diagnostics disabled by default.
            ShowDiagnostics = false;

            //  Input enabled by default.
            IsInputEnabled = true;

            //  Disable special commands by default.
            SendKeyboardCommandsToProcess = false;

            //  Initialise the keymappings.
            InitialiseKeyMappings();

            //  Handle process events.
            _processInterface.OnProcessOutput += ProcessInterfaceOnProcessOutput;
            _processInterface.OnProcessError += ProcessInterfaceOnProcessError;
            _processInterface.OnProcessInput += ProcessInterfaceOnProcessInput;
            _processInterface.OnProcessExit += ProcessInterfaceOnProcessExit;

            //  Wait for key down messages on the rich text box.
            richTextBoxConsole.KeyDown += richTextBoxConsole_KeyDown;
        }

        /// <summary>
        /// Handles the OnProcessError event of the processInterface control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        void ProcessInterfaceOnProcessError(object sender, ProcessEventArgs args)
        {
            //  Write the output, in red
            WriteOutput(args.Content, Color.Red);

            //  Fire the output event.
            FireConsoleOutputEvent(args.Content);
        }

        /// <summary>
        /// Handles the OnProcessOutput event of the processInterface control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        void ProcessInterfaceOnProcessOutput(object sender, ProcessEventArgs args)
        {
            //  Write the output, in white
            WriteOutput(args.Content, Color.White);

            //  Fire the output event.
            FireConsoleOutputEvent(args.Content);
        }

        /// <summary>
        /// Handles the OnProcessInput event of the processInterface control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        void ProcessInterfaceOnProcessInput(object sender, ProcessEventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the OnProcessExit event of the processInterface control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        void ProcessInterfaceOnProcessExit(object sender, ProcessEventArgs args)
        {
            //  Are we showing diagnostics?
            if (ShowDiagnostics)
            {
                WriteOutput(Environment.NewLine + _processInterface.ProcessFileName + " exited.", Color.FromArgb(255, 0, 255, 0));
            }
            
            if (!this.IsHandleCreated)
                return;
            //  Read only again.
            Invoke((Action)(() =>
            {
                richTextBoxConsole.ReadOnly = true;
            }));
        }

        /// <summary>
        /// Initialises the key mappings.
        /// </summary>
        private void InitialiseKeyMappings()
        {
            //  Map 'tab'.
            _keyMappings.Add(new KeyMapping(false, false, false, Keys.Tab, "{TAB}", "\t"));

            //  Map 'Ctrl-C'.
            _keyMappings.Add(new KeyMapping(true, false, false, Keys.C, "^(c)", "\x03\r\n"));
        }

        /// <summary>
        /// Handles the KeyDown event of the richTextBoxConsole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        void richTextBoxConsole_KeyDown(object? sender, KeyEventArgs e)
        {
            //  Check whether we are in the read-only zone.
            var isInReadOnlyZone = richTextBoxConsole.SelectionStart < _inputStart;

            //  Are we sending keyboard commands to the process?
            if (SendKeyboardCommandsToProcess && IsProcessRunning)
            {
                //  Get key mappings for this key event?
                var mappings = from k in _keyMappings
                               where 
                               (k.KeyCode == e.KeyCode &&
                               k.IsAltPressed == e.Alt &&
                               k.IsControlPressed == e.Control &&
                               k.IsShiftPressed == e.Shift)
                               select k;

                //  Go through each mapping, send the message.
                //foreach (var mapping in mappings)
                //{
                    //SendKeysEx.SendKeys(CurrentProcessHwnd, mapping.SendKeysMapping);
                    //inputWriter.WriteLine(mapping.StreamMapping);
//WriteInput("\x3", Color.White, false);
                //}

                //  If we handled a mapping, we're done here.
                if (mappings.Any())
                {
                    e.SuppressKeyPress = true;
                    return;
                }
            }

            //  If we're at the input point and it's backspace, bail.
            if ((richTextBoxConsole.SelectionStart <= _inputStart) && e.KeyCode == Keys.Back) e.SuppressKeyPress = true;

            //  Are we in the read-only zone?
            if (isInReadOnlyZone)
            {
                //  Allow arrows and Ctrl-C.
                if (!(e.KeyCode == Keys.Left ||
                    e.KeyCode == Keys.Right ||
                    e.KeyCode == Keys.Up ||
                    e.KeyCode == Keys.Down ||
                    (e.KeyCode == Keys.C && e.Control)))
                {
                    e.SuppressKeyPress = true;
                }
            }
            
            //  Write the input if we hit return and we're NOT in the read only zone.
            if (e.KeyCode == Keys.Return && !isInReadOnlyZone)
            {
                //  Get the input.
                string input = richTextBoxConsole.Text.Substring(_inputStart, (richTextBoxConsole.SelectionStart) - _inputStart);

                //  Write the input (without echoing).
                WriteInput(input, Color.White, false);
            }
        }

        /// <summary>
        /// Writes the output to the console control.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="color">The color.</param>
        public void WriteOutput(string output, Color color)
        {
            if (string.IsNullOrEmpty(_lastInput) == false && 
                (output == _lastInput || output.Replace("\r\n", "") == _lastInput))
                return;
                
            if (!this.IsHandleCreated)
                return;

            Invoke((Action)(() =>
            {
                //  Write the output.
                richTextBoxConsole.SelectionColor = color;
                richTextBoxConsole.SelectedText += output;
                _inputStart = richTextBoxConsole.SelectionStart;
            }));
        }

        /// <summary>
        /// Clears the output.
        /// </summary>
        public void ClearOutput()
        {
            richTextBoxConsole.Clear();
            _inputStart = 0;
        }

        /// <summary>
        /// Writes the input to the console control.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="color">The color.</param>
        /// <param name="echo">if set to <c>true</c> echo the input.</param>
        public void WriteInput(string input, Color color, bool echo)
        {
            Invoke((Action)(() =>
            {
                //  Are we echoing?
                if (echo)
                {
                    richTextBoxConsole.SelectionColor = color;
                    richTextBoxConsole.SelectedText += input;
                    _inputStart = richTextBoxConsole.SelectionStart;
                }

                _lastInput = input;

                //  Write the input.
                _processInterface.WriteInput(input);

                //  Fire the event.
                FireConsoleInputEvent(input);
            }));
        }

        

        /// <summary>
        /// Runs a process.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="arguments">The arguments.</param>
        public void StartProcess(string fileName, string arguments)
        {
            //  Are we showing diagnostics?
            if (ShowDiagnostics)
            {
                WriteOutput("Preparing to run " + fileName, Color.FromArgb(255, 0, 255, 0));
                if (!string.IsNullOrEmpty(arguments))
                    WriteOutput(" with arguments " + arguments + "." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
                else
                    WriteOutput("." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
            }

            //  Start the process.
            _processInterface.StartProcess(fileName, arguments);

            //  If we enable input, make the control not read only.
            if (IsInputEnabled)
                richTextBoxConsole.ReadOnly = false;
        }

        /// <summary>
        /// Runs a process.
        /// </summary>
        /// <param name="processStartInfo"><see cref="ProcessStartInfo"/> to pass to the process.</param>
        public void StartProcess(ProcessStartInfo processStartInfo)
        {
            //  Are we showing diagnostics?
            if (ShowDiagnostics)
            {
                WriteOutput("Preparing to run " + processStartInfo.FileName, Color.FromArgb(255, 0, 255, 0));
                if (!string.IsNullOrEmpty(processStartInfo.Arguments))
                    WriteOutput(" with arguments " + processStartInfo.Arguments + "." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
                else
                    WriteOutput("." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
            }

            //  Start the process.
            _processInterface.StartProcess(processStartInfo);

            //  If we enable input, make the control not read only.
            if (IsInputEnabled)
                richTextBoxConsole.ReadOnly = false;
        }

        /// <summary>
        /// Stops the process.
        /// </summary>
        public void StopProcess()
        {
            //  Stop the interface.
            _processInterface.StopProcess();
        }
        
        /// <summary>
        /// Fires the console output event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireConsoleOutputEvent(string content)
        {
            //  Get the event.
            if (OnConsoleOutput is not null)
                OnConsoleOutput(this, new ConsoleEventArgs(content));
        }

        /// <summary>
        /// Fires the console input event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireConsoleInputEvent(string content)
        {
            //  Get the event.
            if (OnConsoleInput is not null)
                OnConsoleInput(this, new ConsoleEventArgs(content));
        }

        /// <summary>
        /// The internal process interface used to interface with the process.
        /// </summary>
        private readonly ProcessInterface _processInterface = new();
        
        /// <summary>
        /// Current position that input starts at.
        /// </summary>
        private int _inputStart = -1;

        /// <summary>
        /// The is input enabled flag.
        /// </summary>
        private bool _isInputEnabled = true;

        /// <summary>
        /// The last input string (used so that we can make sure we don't echo input twice).
        /// </summary>
        private string _lastInput = string.Empty;

        /// <summary>
        /// The key mappings.
        /// </summary>
        private readonly List<KeyMapping> _keyMappings = new();

        /// <summary>
        /// Occurs when console output is produced.
        /// </summary>
        public event ConsoleEventHandler? OnConsoleOutput;

        /// <summary>
        /// Occurs when console input is produced.
        /// </summary>
        public event ConsoleEventHandler? OnConsoleInput;

        /// <summary>
        /// Gets or sets a value indicating whether to show diagnostics.
        /// </summary>
        /// <value>
        ///   <c>true</c> if show diagnostics; otherwise, <c>false</c>.
        /// </value>
        [Category("Console Control"), Description("Show diagnostic information, such as exceptions.")]
        public bool ShowDiagnostics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is input enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is input enabled; otherwise, <c>false</c>.
        /// </value>
        [Category("Console Control"), Description("If true, the user can key in input.")]
        public bool IsInputEnabled
        {
            get { return _isInputEnabled; }
            set
            {
                _isInputEnabled = value;
                if (IsProcessRunning)
                    richTextBoxConsole.ReadOnly = !value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [send keyboard commands to process].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [send keyboard commands to process]; otherwise, <c>false</c>.
        /// </value>
        [Category("Console Control"), Description("If true, special keyboard commands like Ctrl-C and tab are sent to the process.")]
        public bool SendKeyboardCommandsToProcess
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is process running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is process running; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public bool IsProcessRunning
        {
            get { return _processInterface.IsProcessRunning; }
        }

        /// <summary>
        /// Gets the internal rich text box.
        /// </summary>
        [Browsable(false)]
        public RichTextBox InternalRichTextBox
        {
            get { return richTextBoxConsole; }
        }

        /// <summary>
        /// Gets the process interface.
        /// </summary>
        [Browsable(false)]
        public ProcessInterface ProcessInterface
        {
            get { return _processInterface; }
        }

        /// <summary>
        /// Gets the key mappings.
        /// </summary>
        [Browsable(false)]
        public List<KeyMapping> KeyMappings
        {
            get { return _keyMappings; }
        }

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        /// <returns>The <see cref="T:System.Drawing.Font" /> to apply to the text displayed by the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultFont" /> property.</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        public override Font Font
        {
            get
            {
                //  Return the base class font.
                return base.Font;
            }
            set
            {
                //  Set the base class font...
                base.Font = value;

                //  ...and the internal control font.
                richTextBoxConsole.Font = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color for the control.
        /// </summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the background color of the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultBackColor" /> property.</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        public override Color BackColor
        {
            get
            {
                //  Return the base class background.
                return base.BackColor;
            }
            set
            {
                //  Set the base class background...
                base.BackColor = value;

                //  ...and the internal control background.
                richTextBoxConsole.BackColor = value;
            }
        }
    }

    /// <summary>
    /// Used to allow us to find resources properly.
    /// </summary>
    public class Resfinder {}
}
