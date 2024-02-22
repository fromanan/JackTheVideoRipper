namespace JackTheVideoRipper
{
    public partial class FrameConvert : Form
    {
        public FrameConvert()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs args)
        {
            FileSystem.GetWebResourceHandle(Urls.Handbrake, useShellExecute:false);
        }
    }
}
