namespace JackTheVideoRipper
{
    public partial class FrameCheckMetadata : Form
    {
        public FrameCheckMetadata()
        {
            InitializeComponent();
        }

        private void timerPostLoad_Tick(object sender, EventArgs args)
        {
            // Timeout
            Close();
        }
    }
}
