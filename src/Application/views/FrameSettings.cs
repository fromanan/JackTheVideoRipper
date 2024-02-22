using JackTheVideoRipper.framework;

namespace JackTheVideoRipper
{
   public partial class FrameSettings : Form
   {
      #region Data Members

      public static event Action SettingsUpdatedEvent = delegate { };

      #endregion

      #region Properties

      private static SettingsModel CurrentSettings => Settings.Data;

      #endregion

      #region Form View Accessors

      public int MaxConcurrentDownloads
      {
         get => (int)numMaxConcurrent.Value;
         set => numMaxConcurrent.Value = value;
      }

      public string DefaultDownloadPath
      {
         get => textLocation.Text.Trim();
         set => textLocation.Text = value;
      }

      public string TempFolderPath
      {
         get => textTempFolder.Text.Trim();
         set => textTempFolder.Text = value;
      }

      public bool SkipMetadata
      {
         get => skipMetadata.Checked;
         set => skipMetadata.Checked = value;
      }

      public bool SimpleDownloads
      {
         get => simplifiedDownload.Checked;
         set => simplifiedDownload.Checked = value;
      }

      public bool StoreHistory
      {
         get => storeHistory.Checked;
         set => storeHistory.Checked = value;
      }

      public bool EnableDeveloperMode
      {
         get => enableDeveloperMode.Checked;
         set => enableDeveloperMode.Checked = value;
      }

      public bool EnableMultithreadedDownloads
      {
         get => enableMultithreadedDownloads.Checked;
         set => enableMultithreadedDownloads.Checked = value;
      }

      #endregion

      #region Constructor

      public FrameSettings()
      {
         InitializeComponent();
         SubscribeEvents();
      }

      #endregion

      #region Private Methods

      private void LoadSettings()
      {
         MaxConcurrentDownloads = CurrentSettings.MaxConcurrentDownloads;
         DefaultDownloadPath = CurrentSettings.DefaultDownloadPath;
         TempFolderPath = CurrentSettings.TempFolderPath;
         SimpleDownloads = CurrentSettings.SimpleDownloads;
         SkipMetadata = CurrentSettings.SkipMetadata;
         StoreHistory = CurrentSettings.StoreHistory;
         EnableDeveloperMode = CurrentSettings.EnableDeveloperMode;
         EnableMultithreadedDownloads = CurrentSettings.EnableMultiThreadedDownloads;
      }

      private void SetSettings()
      {
         CurrentSettings.MaxConcurrentDownloads = MaxConcurrentDownloads;
         CurrentSettings.DefaultDownloadPath = DefaultDownloadPath;
         CurrentSettings.TempFolderPath = TempFolderPath;
         CurrentSettings.SimpleDownloads = SimpleDownloads;
         CurrentSettings.SkipMetadata = SkipMetadata;
         CurrentSettings.StoreHistory = StoreHistory;
         CurrentSettings.EnableDeveloperMode = EnableDeveloperMode;
         CurrentSettings.EnableMultiThreadedDownloads = EnableMultithreadedDownloads;

         SettingsUpdatedEvent();
      }

      private static void Save()
      {
         Settings.Save();
      }

      #endregion

      #region Form Events

      private void FrameSettings_Load(object sender, EventArgs e)
      {
         LoadSettings();
      }

      private void SubscribeEvents()
      {
         buttonLocationBrowse.Click += (_, _) =>
         {
            if (FileSystem.SelectFolder(DefaultDownloadPath) is { } selectedPath)
               DefaultDownloadPath = selectedPath;
         };

         buttonTempFolderBrowse.Click += (_, _) =>
         {
            if (FileSystem.SelectFolder(TempFolderPath) is { } selectedPath)
               TempFolderPath = selectedPath;
         };

         buttonSave.Click += (_, _) =>
         {
            SetSettings();
            Save();
            Close();
         };
      }

      #endregion
   }
}
