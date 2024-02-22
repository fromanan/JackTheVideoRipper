using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.modules;

namespace JackTheVideoRipper.models.rows;

public class RecodeProcessUpdateRow : FFMPEGProcessRow
{
    public override MediaProcessType ProcessType { get; init; } = MediaProcessType.Recode;

    public override FFMPEG.Operation OperationType { get; init; } = FFMPEG.Operation.Recode;
    
    public RecodeProcessUpdateRow(IMediaItem mediaItem, Action<IProcessRunner> completionCallback) :
        base(mediaItem, completionCallback) { }

    protected override string GetStatus()
    {
        return Messages.Compressing;
    }
}