using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.modules;

namespace JackTheVideoRipper.models.rows;

public class RepairProcessUpdateRow : FFMPEGProcessRow
{
    public override MediaProcessType ProcessType { get; init; } = MediaProcessType.Repair;

    public override FFMPEG.Operation OperationType { get; init; } = FFMPEG.Operation.Repair;
    
    public RepairProcessUpdateRow(IMediaItem mediaItem, Action<IProcessRunner> completionCallback) :
        base(mediaItem, completionCallback) { }

    protected override string GetStatus()
    {
        return Messages.Compressing;
    }
}