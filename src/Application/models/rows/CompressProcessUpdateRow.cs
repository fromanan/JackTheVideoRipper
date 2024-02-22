using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.modules;

namespace JackTheVideoRipper.models.rows;

public class CompressProcessUpdateRow : FFMPEGProcessRow
{
    public override MediaProcessType ProcessType { get; init; } = MediaProcessType.Compress;

    public override FFMPEG.Operation OperationType { get; init; } = FFMPEG.Operation.Compress;
    
    public CompressProcessUpdateRow(IMediaItem mediaItem, Action<IProcessRunner> completionCallback) :
        base(mediaItem, completionCallback) { }

    protected override string GetStatus()
    {
        return Messages.Compressing;
    }
}