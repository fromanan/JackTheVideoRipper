using System.Diagnostics;
using JackTheVideoRipper.extensions;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.containers;
using JackTheVideoRipper.modules;

namespace JackTheVideoRipper.models;

public class CompressProcessUpdateRow : ProcessUpdateRow
{
    private int _totalFrames;

    private readonly ExifData _exifData = new();
    
    public CompressProcessUpdateRow(IMediaItem mediaItem, Action<IProcessRunner> completionCallback) :
        base(mediaItem, completionCallback)
    {
        Task.Run(LoadMetadata);
    }

    protected override Process CreateProcess()
    {
        return FFMPEG.CreateCommand(ParameterString);
    }

    public override async Task<bool> Start()
    {
        SetViewField(() => Url = Text.NotApplicable);
        return await base.Start();
    }

    protected override void SetProgressText(IReadOnlyList<string> tokens)
    {
        if (tokens.Count < 8)
            return;

        if (!tokens[0].Contains("frame"))
            return;

        FfmpegFrame ffmpegFrame = new(tokens);
        Progress = CalculateProgress(ffmpegFrame.Frame);
        Eta = CalculateEta(ffmpegFrame.Frame, ffmpegFrame.Fps);
        FileSize = FileSystem.GetFileSizeFormatted(ffmpegFrame.Size);
        Speed = $"{ffmpegFrame.Fps} fps";
    }

    // If over 100%, change message to finalizing download tasks?
    private string CalculateProgress(int frame)
    {
        if (_totalFrames <= 0)
            return Text.DefaultProgress;

        float progress = frame * 100f / _totalFrames;
        
        return progress > 100 ? Text.ProgressComplete : $"{progress:F2}%";
    }

    private string CalculateEta(int frame, float fps)
    {
        return Common.TimeString(((float) _totalFrames - frame) / fps);
    }

    protected override async Task<string> GetTitle()
    {
        return await ExifTool.GetTag(Filepath, "Title");
    }

    private static bool IsFileExistsLine(IEnumerable<string> tokens)
    {
        return IsFileExistsLine(tokens.Merge(" "));
    }
    
    private static bool IsFileExistsLine(string line)
    {
        return line.StartsWith("File") && line.EndsWith(Messages.FFMPEGFileExists);
    }

    protected override string GetStatus()
    {
        return Messages.Compressing;
    }
    
    private async Task LoadMetadata()
    {
        _exifData.LoadData(await ExifTool.GetMetadataString(Filepath));
        _totalFrames = _exifData.Frames > 0 ? _exifData.Frames : await FFMPEG.GetNumberOfFrames(Filepath);
    }

    {
    }
}