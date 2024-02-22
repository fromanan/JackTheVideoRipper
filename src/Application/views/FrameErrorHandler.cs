using System.Media;
using JackTheVideoRipper.extensions;

namespace JackTheVideoRipper.views
{
    public partial class FrameErrorHandler : Form
    {
        private readonly Exception _exception;
        
        public FrameErrorHandler(Exception exception)
        {
            _exception = exception.InnerException ?? exception;;
            InitializeComponent();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            Load += FrameErrorHandler_Load;
            bContinue.Click += OnContinueClicked;
            bQuit.Click += OnQuitClicked;
            bSaveDetails.Click += OnSaveDetailsClicked;
        }
        
        private void FrameErrorHandler_Load(object? sender, EventArgs args)
        {
            Text = $@"Uncaught Exception | {_exception}";
            SetExceptionInformation();
            SystemSounds.Exclamation.Play();
        }

        private void SetExceptionInformation()
        {
            tbMessage.Text = _exception.Message;
            tbSource.Text = _exception.Source;
            tbCaller.Text = _exception.GetCaller();
            tbType.Text = _exception.GetBaseTypeName();
            rtbStackTrace.Text = _exception.StackTrace;
        }

        private void OnContinueClicked(object? sender, EventArgs args)
        {
            this.Close(DialogResult.Continue);
        }

        private void OnQuitClicked(object? sender, EventArgs args)
        {
            this.Close(DialogResult.Abort);
        }

        private void OnSaveDetailsClicked(object? sender, EventArgs args)
        {
            _exception.SaveToFile();
        }
    }
}
