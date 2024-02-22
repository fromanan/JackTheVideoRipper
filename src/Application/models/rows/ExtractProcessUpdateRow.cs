using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.modules;

namespace JackTheVideoRipper.models.rows;

public class ExtractProcessUpdateRow : FFMPEGProcessRow
{
    public override MediaProcessType ProcessType { get; init; } = MediaProcessType.Extract;

    public override FFMPEG.Operation OperationType { get; init; } = FFMPEG.Operation.Extract;
    
    public ExtractProcessUpdateRow(IMediaItem mediaItem, Action<IProcessRunner> completionCallback) :
        base(mediaItem, completionCallback) { }

    protected override string GetStatus()
    {
        return Messages.Compressing;
    }
}