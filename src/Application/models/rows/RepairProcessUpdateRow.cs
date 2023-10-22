using System.Diagnostics;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.processes;

namespace JackTheVideoRipper.models.rows;

public class RepairProcessUpdateRow : ProcessUpdateRow
{
    public RepairProcessUpdateRow(IMediaItem mediaItem, Action<IProcessRunner> completionCallback) :
        base(mediaItem, completionCallback)
    {
    }

    protected override Process CreateProcess()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetTitle()
    {
        throw new NotImplementedException();
    }

    protected override RowUpdateArgs? SetProgressText(IReadOnlyList<string> tokens)
    {
        throw new NotImplementedException();
    }

    protected override string? GetStatus()
    {
        throw new NotImplementedException();
    }
}