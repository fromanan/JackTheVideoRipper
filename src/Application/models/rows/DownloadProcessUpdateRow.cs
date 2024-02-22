using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using JackTheVideoRipper.extensions;
using JackTheVideoRipper.framework;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.libraries;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.modules;

namespace JackTheVideoRipper.models.rows;

public class DownloadProcessUpdateRow : ProcessUpdateRow
{
    #region Data Members

    private bool _redirected;

    private string _redirectedUrl = string.Empty;

    private DownloadStage _downloadStage = DownloadStage.None;

    public readonly Provider Provider;
    
    private static readonly Regex _MoveFilePattern = new("Moving file \"(?<src>[^\"]+)\" to \"(?<dest>[^\"]+)\"");
    
    public override MediaProcessType ProcessType { get; init; } = MediaProcessType.Download;

    #endregion

    #region Properties
    
    public string OriginalUrl => base.Url;
    
    public new string Url
    {
        get => _redirected ? _redirectedUrl : OriginalUrl;
        set
        {
            _redirected = true;
            _redirectedUrl = value;
        }
    }

    public new string ParameterString => _redirected ?
        base.ParameterString.Replace(OriginalUrl, Url) :
        base.ParameterString;

    private bool InvalidFilesize => FileSize.IsNullOrEmpty() || FileSize is "-" or "~" || FilesizeNegligible;
    
    private bool FilesizeNegligible => !float.TryParse(FileSize.Split()[0], out float size) || size < 0.001f;

    #endregion

    #region Constructor

    public DownloadProcessUpdateRow(IMediaItem mediaItem, Action<IProcessRunner> completionCallback) :
        base(mediaItem, completionCallback)
    {
        Provider = ProviderMap.LookupFromDomain(mediaItem.Url);
    }
        
    #endregion

    #region Overrides
    
    protected override Process CreateProcess()
    {
        return YouTubeDL.CreateCommand(ParameterString);
    }

    // Extract elements of CLI output from YouTube-DL
    protected override RowUpdateArgs? SetProgressText(IReadOnlyList<string> tokens)
    {
        if (tokens.Count < 8 || _downloadStage != DownloadStage.Downloading)
            return default;
        
        // TODO: Remove these in favor of task-based update
        /*Progress = tokens[1];
        FileSize = FormatSize(tokens[3]);
        Speed = tokens[5];
        Eta = tokens[7];*/

        return new RowUpdateArgs
        {
            Tag = Tag,
            Progress = tokens[1],
            FileSize = FormatSize(tokens[3]),
            Speed = tokens[5],
            Eta = tokens[7]
        };
    }
    
    protected override async Task<string> GetTitle()
    {
        return await YouTubeDL.GetTitle(Url, false);
    }

    protected override string? GetStatus()
    {
        if (Buffer.ProcessLine is not { } line || line.IsNullOrEmpty())
            return string.Empty;

        _downloadStage = YouTubeDL.GetDownloadStage(line);

        return _downloadStage switch
        {
            DownloadStage.Waiting       => Messages.Waiting,
            DownloadStage.Retrieving    => Messages.Retrieving,
            DownloadStage.Transcoding   => Messages.Transcoding,
            DownloadStage.Downloading   => Messages.Downloading,
            DownloadStage.Metadata      => Messages.SavingMetadata,
            DownloadStage.Error         => null,
            _                           => string.Empty
        };
    }

    public override async Task<bool> Start()
    {
        // Called before process starts
        if (!await PreDownloadTasks())
            return false;

        if (!YouTubeDL.IsSupported(Url))
        {
            string? domain = FileSystem.ParseUrl(Url)?.Domain.WrapQuotes();
            Fail(new YouTubeDL.LinkNotSupportedException(string.Format(Messages.LinkNotSupported, domain)));
            return false;
        }
        
        return await base.Start();
    }

    public override void OnProcessExit(object? o, EventArgs eventArgs)
    {
        base.OnProcessExit(o, eventArgs);
        
        // Editing the GUI elements must be in the main thread context to avoid errors
        Threading.RunInMainContext(PostDownloadTasks);
    }

    #endregion

    #region Public Methods

    public string GetFilepath()
    {
        // TODO: Need to fix for non-youtube providers
        if (Provider is not Provider.YouTube)
            return Text.NotApplicable;

        if (Buffer.Contains(Messages.YouTubeDLAlreadyDownloaded, StringComparison.OrdinalIgnoreCase))
        {
            return Buffer.GetResultWhichContains(Tags.DOWNLOAD)
                .ValueOrDefault()
                .After(Tags.DOWNLOAD)
                .Before(Messages.YouTubeDLAlreadyDownloaded)
                .Trim();
        }
        
        if (Buffer.GetResultWhichContains(Tags.MOVE_FILES) is not { } pathString)
            return Text.NotApplicable;
        
        string line = pathString.After(Tags.MOVE_FILES).Trim();

        Match match = _MoveFilePattern.Match(line);

        return match.Groups.ContainsKey("dest") ? match.Groups["dest"].Value : Text.NotApplicable;
    }

    #endregion

    #region Private Methods

    private async Task<bool> PreDownloadTasks()
    {
        if (FileSystem.ParseUrl(OriginalUrl) is not { } domainInfo || !VideoProxy.IsProxyLink(domainInfo))
            return true;

        try
        {
            _redirectedUrl = await GetRedirectedLink(VideoProxy.GetProxyType(domainInfo.Domain), OriginalUrl);
        }
        catch (Exception exception)
        {
            Fail(exception);
            return false;
        }

        HttpResponseMessage resourceStatus = await Web.GetResourceStatus(_redirectedUrl);

        switch (resourceStatus.StatusCode)
        {
            case HttpStatusCode.NotFound:
                string message = string.Format(Messages.ResourceNotFound, _redirectedUrl.WrapQuotes(),
                    resourceStatus.ResponseCode());
                Fail(new WebException(message));
                return false;
        }
        
        _redirected = _redirectedUrl != OriginalUrl;
        if (_redirected)
        {
            string oldSite = domainInfo.Domain.WrapQuotes();
            string newSite = FileSystem.ParseUrl(_redirectedUrl)?.Domain.WrapQuotes().ValueOrDefault()!;
            Buffer.AddLog(string.Format(Messages.RedirectedProxy, oldSite, newSite), ProcessLogType.Info);
        }
        
        return true;
    }

    private static async Task<string> GetRedirectedLink(VideoProxyType proxyType, string url)
    {
        return proxyType switch
        {
            _ => url
        };
    }

    private async Task PostDownloadTasks()
    {
        Path = GetFilepath();

        if (InvalidFilesize)
            FileSize = FileSystem.GetFileSizeFormatted(Path);

        if (Title.IsNullOrEmpty())
            await RetrieveTitle();

        // Add title metadata
        await ExifTool.AddTag(Path, "Title", Title);

        SendProcessCompletedNotification();
            
        // Update history information
        History.Data.UpdateFileInformation(Tag, Path, FileSize);
    }

    private static string FormatSize(string size)
    {
        if (size.Contains("KiB"))
            return size.Replace("KiB", " KB");
        if (size.Contains("MiB"))
            return size.Replace("MiB", " MB");
        if (size.Contains("GiB"))
            return size.Replace("GiB", " GB");
        if (size.Contains("TeB"))
            return size.Replace("TeB", " TB");
        return size;
    }

    private void SendProcessCompletedNotification()
    {
        if (Succeeded)
        {
            string notificationMessage = string.Format(Messages.FinishedDownloading, Title.WrapQuotes());
            string shortenedMessage = string.Format(Messages.FinishedDownloading, Title.TruncateEllipse(35).WrapQuotes());
            NotificationsManager.SendNotification(new Notification(notificationMessage, this, shortenedMessage));
        }
        else if (GetError() is { } errorMessage && errorMessage.HasValue())
        {
            NotificationsManager.SendNotification(new Notification(string.Format(Messages.FailedToDownload, errorMessage), this));
        }
    }

    #endregion
}