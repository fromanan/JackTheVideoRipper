﻿using JackTheVideoRipper.extensions;
using JackTheVideoRipper.models;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.Properties;

namespace JackTheVideoRipper
{
    public partial class FrameNewMedia : Form
    {
        #region Data Members

        public MediaItemRow MediaItemRow = new();

        private readonly MediaType _startType;
        private string? _lastValidUrl;
        
        private readonly FormatManager _formatManager = new();

        #endregion

        #region Properties

        private bool FormatVideo => cbVideoFormat.Enabled && cbVideoEncoder.SelectedIndex > 0;

        private bool FormatAudio => cbAudioEncoder.Enabled && cbAudioEncoder.SelectedIndex > 0;
        
        private bool IsValidVideo => VideoExtension.HasValueAndNot("mp4");

        private bool IsValidAudio => AudioExtension.HasValueAndNot("mp3", "m4a");
        
        private bool ShouldUpdateAudio => ExportAudio && !FormatVideo;
        
        private string Filename => FileSystem.GetFilename(Filepath);

        #endregion

        #region Form Element Accessors

        private string Title
        {
            get => labelTitle.Text.Trim();
            set => labelTitle.Text = value;
        }

        private string Url
        {
            get => textUrl.Text.Trim();
            set => textUrl.Text = value;
        }

        private string Filepath
        {
            get => textLocation.Text.Trim();
            set => textLocation.Text = value;
        }

        private string Description
        {
            get => labelDescription.Text.Trim();
            set => labelDescription.Text = value;
        }
        
        private bool ExportAudio
        {
            get => chkBoxExportAudio.Checked;
            set => chkBoxExportAudio.Checked = value;
        }
        
        private bool ExportVideo
        {
            get => chkBoxExportVideo.Checked;
            set => chkBoxExportVideo.Checked = value;
        }

        private string AudioEncoder => cbAudioEncoder.Text.Trim();

        private string VideoEncoder => cbVideoEncoder.Text.Trim();

        private string AudioFormat => cbAudioFormat.Text.Trim();

        private string VideoFormat => cbVideoFormat.Text.Trim();

        private string AudioExtension => cbAudioEncoder.Text.Trim();
        
        private string VideoExtension => cbVideoEncoder.Text.Trim();
        
        private ComboBox.ObjectCollection VideoFormatItems => cbVideoFormat.Items;
        private ComboBox.ObjectCollection AudioFormatItems => cbAudioFormat.Items;
        
        private string Username => textUsername.Text;

        private string Password => textPassword.Text;
        
        private bool WriteMetaData => chkBoxWriteMetadata.Checked;

        private bool IncludeAds => chkBoxIncludeAds.Checked;
        
        private bool EmbedThumbnail => chkBoxEmbedThumbnail.Checked;

        private bool EmbedSubtitles => chkEmbedSubs.Checked;

        private string VideoFormatId => _formatManager.GetVideoFormatId(VideoFormat)!;
        
        private string AudioFormatId => _formatManager.GetAudioFormatId(AudioFormat)!;
        
        #endregion

        #region Constructor

        public FrameNewMedia(MediaType type)
        {
            _startType = type;
            InitializeComponent();
        }

        #endregion

        private void AddVideoFormats()
        {
            VideoFormatItems.AddRange(_formatManager.GetVideoFormatRows().ToArray<object>());
            
            if (VideoFormatItems.Count < 2)
                VideoFormatItems.Add("(no video metadata could be extracted)");
            
            cbVideoFormat.SelectedIndex = cbVideoEncoder.Items.Count > 0 ? 0 : 1;
        }

        private void AddAudioFormats()
        {
            AudioFormatItems.AddRange(_formatManager.GetAudioFormatRows().ToArray<object>());
            
            if (AudioFormatItems.Count < 2)
                AudioFormatItems.Add("(no audio metadata could be extracted)");
            
            cbAudioFormat.SelectedIndex = cbAudioEncoder.Items.Count > 0 ? 0 : 1;
        }

        private void UpdateFormatViewItems()
        {
            AddVideoFormats();

            AddAudioFormats();
        }

        private void IngestMediaUrl()
        {
            try { RetrieveMetadata(Url); }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Modals.Error(@"Unable to retrieve metadata!");
            }
        }

        private bool ValidateUrl(string url)
        {
            if (url == _lastValidUrl || !Common.IsValidUrl(url))
                return false;
            
            _lastValidUrl = url;

            return true;
        }

        private void RetrieveMetadata(string url)
        {
            if (!Common.IsValidUrl(url))
                return;
            
            FrameCheckMetadata frameCheckMetadata = new();
            
            Enabled = false;
            
            frameCheckMetadata.Show();
            Application.DoEvents();

            try { ExtractMediaInfo(); }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Modals.Error(@"Unable to retrieve metadata!");
            }
            
            frameCheckMetadata.Close();
            Enabled = true;
        }

        private void ClearFormatItems()
        {
            VideoFormatItems.Clear();
            AudioFormatItems.Clear();
        }
        
        public void ExtractMediaInfo()
        {
            // Meta data lookup failed (happens on initial lookup)
            if (YouTubeDL.GetMediaData(Url) is not { } info)
                return;

            if (info.Formats is {Count: > 0})
                RefreshFormatViews(info);

            UpdateMediaPreview(info.MetaData);
        }

        private void RefreshFormatViews(MediaInfoData mediaInfoData)
        {
            ClearFormatItems();
            _formatManager.UpdateAvailableFormats(mediaInfoData);
            UpdateFormatViewItems();
        }

        private void UpdateMediaPreview(MetaData metaData)
        {
            pbPreview.ImageLocation = metaData.ThumbnailFilepath;
            Title = metaData.Title;
            Description = metaData.Description;
            Filepath = metaData.MediaFilepath;
        }

        private void CopyUrlToClipboard()
        {
            string clipboard = FileSystem.GetClipboardText();
            if (!Common.IsValidUrl(clipboard)) 
                return;
            Url = clipboard;
        }

        private void UpdateFormat(string formatExtension)
        {
            Filepath = $"{Filename}{formatExtension.Split('/')[2].Trim()}";
        }

        private bool ValidateMedia()
        {
            if (!EmbedThumbnail)
                return true;
            
            if (FormatVideo && IsValidVideo)
            {
                Modals.Error(Resources.InvalidVideo);
                return false;
            }
            
            if (FormatAudio && IsValidAudio)
            {
                Modals.Error(Resources.InvalidAudio);
                return false;
            }

            return true;
        }

        private void GenerateDownloadCommand()
        {
            ValidateMedia();

            GenerateMediaItemRow();
        }

        private void GenerateMediaItemRow()
        {
            MediaItemRow = new MediaItemRow
            {
                Url = Url,
                Filepath = Filepath,
                Type = ExportVideo ? MediaType.Video : MediaType.Audio,
                Title = Title,
                Parameters = new Parameters
                {
                    MediaSourceUrl = Url,
                    FilenameFormatted = Filepath,
                    Username = Username,
                    Password = Password,
                    EncodeVideo = FormatVideo,
                    AddMetaData = WriteMetaData,
                    IncludeAds = IncludeAds,
                    EmbedThumbnail = EmbedThumbnail,
                    EmbedSubtitles = EmbedSubtitles,
                    ExportAudio = ExportAudio,
                    ExportVideo = ExportVideo,
                    AudioFormat = AudioExtension,
                    VideoFormat = VideoExtension,
                    VideoFormatId = VideoFormatId,
                    AudioFormatId = AudioFormatId
                }
            };
        }

        private void ToggleVideo(bool enabled)
        {
            cbVideoFormat.Enabled = enabled;
            cbVideoEncoder.Enabled = enabled;
            cbAudioEncoder.Enabled = !enabled;
        }
        
        private void ToggleAudio(bool enabled)
        {
            cbAudioFormat.Enabled = enabled;
            cbAudioEncoder.Enabled = enabled && !ExportVideo;
            cbVideoEncoder.Enabled = !enabled || ExportVideo;
        }

        #region Timer Events

        private void TimerPostLoad_Tick(object sender, EventArgs e)
        {
            timerPostLoad.Enabled = false;
            CopyUrlToClipboard();
            TextUrl_TextChanged(sender, e);
        }

        #endregion

        #region Event Handlers
        
        private void FrameNewMedia_Load(object sender, EventArgs e)
        {
            if (_startType == MediaType.Audio)
                ExportVideo = false;
        }
        
        private void ButtonDownload_Click(object sender, EventArgs e)
        {
            if (Url.HasValue())
            {
                GenerateDownloadCommand();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                // TODO?
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButtonLocationBrowse_Click(object sender, EventArgs e)
        {
            if (Url.IsNullOrEmpty() || Filepath.IsNullOrEmpty()) 
                return;

            if (FileSystem.SaveCopy(Filepath) is { } result && result.HasValue())
                Filepath = result;
        }

        private async void TextUrl_TextChanged(object sender, EventArgs e)
        {
            Input.WaitForFinishTyping(() => Url);

            if (!ValidateUrl(Url))
                return;

            RetrieveMetadata();

            FileSystem.WarnIfFileExists(Filepath);
        }

        private void ChkBoxExportAudio_CheckedChanged(object sender, EventArgs e)
        {
            if (!ExportAudio && !ExportVideo)
                ExportAudio = true;
            
            if (ExportAudio)
            {
                ToggleAudio(true);
                CbAudioEncoder_TextChanged(sender, e); // TODO: fix
            }
            else
            {
                ToggleAudio(false);
            }
        }

        private void ChkBoxExportVideo_CheckedChanged(object sender, EventArgs e)
        {
            // We must export one of the two
            if (!ExportVideo && !ExportAudio)
                ExportVideo = true;

            if (ExportVideo)
            {
                ToggleVideo(true);
                CbVideoEncoder_TextChanged(sender, e); // TODO: fix
            }
            else
            {
                ToggleVideo(false);
                tabImportType.SelectedTab = tabPageAudio;
            }
        }

        private void CbVideoEncoder_TextChanged(object sender, EventArgs e)
        {
            if (!FormatVideo) 
                return;
            Filepath = $"{Filename}.{VideoExtension}";
        }
        
        private void CbVideoFormat_TextChanged(object sender, EventArgs e)
        {
            if (!FormatVideo) 
                return;
            UpdateFormat(VideoFormat);
        }
        
        private void CbAudioEncoder_TextChanged(object sender, EventArgs e)
        {
            if (ShouldUpdateAudio)
                return;
            Filepath = $"{Filename}.{AudioExtension}";
        }

        private void CbAudioFormat_TextChanged(object sender, EventArgs e)
        {
            if (ShouldUpdateAudio) 
                return;
            UpdateFormat(AudioFormat);
        }

        private void ButtonGetCommand_Click(object sender, EventArgs e)
        {
            if (Url.HasValue())
            {
                GenerateDownloadCommand();
                
                FileSystem.SetClipboardText($"{YouTubeDL.ExecutablePath} {MediaItemRow.ParameterString}");

                Modals.Notification(@"Command copied to clipboard!", @"Generate Command");
            }
            else
            {
                // TODO?
            }
        }
        
        // Can't select the preview rows
        private void CbVideoFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbVideoFormat.SelectedIndex == 0)
                cbVideoFormat.SelectedIndex = 1;
        }

        private void CbAudioFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbAudioFormat.SelectedIndex == 0)
                cbAudioFormat.SelectedIndex = 1;
        }

        #endregion
    }
}
