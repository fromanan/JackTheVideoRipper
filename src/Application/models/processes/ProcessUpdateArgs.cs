using JackTheVideoRipper.models.rows;

namespace JackTheVideoRipper.models.processes;

public record ProcessUpdateArgs(bool Completed, RowUpdateArgs? RowUpdateArgs = null)
{
    public static readonly ProcessUpdateArgs Default = new(false);

    public static readonly ProcessUpdateArgs Done = new(true);
}