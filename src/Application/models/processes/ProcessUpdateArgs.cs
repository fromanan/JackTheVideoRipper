using JackTheVideoRipper.models.rows;

namespace JackTheVideoRipper.models.processes;

public record ProcessUpdateArgs(object? Sender, bool Completed, RowUpdateArgs? RowUpdateArgs = null)
{
    public static ProcessUpdateArgs Default(object? sender = null) => new(sender, false);

    public static ProcessUpdateArgs Done(object? sender = null) => new(sender, true);
}