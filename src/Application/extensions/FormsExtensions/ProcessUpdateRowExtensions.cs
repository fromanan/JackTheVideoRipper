namespace JackTheVideoRipper.extensions;

public static class ProcessUpdateRowExtensions
{
    public static void Pause(this IProcessUpdateRowEnumerable processUpdateRows)
    {
        Parallel.ForEach(processUpdateRows, p => p.Pause());
    }
    
    public static void Resume(this IProcessUpdateRowEnumerable processUpdateRows)
    {
        Parallel.ForEach(processUpdateRows, p => p.Resume());
    }

    public static void Kill(this IProcessUpdateRowEnumerable processUpdateRows)
    {
        Parallel.ForEach(processUpdateRows, p => p.Kill());
    }

    public static void Stop(this IProcessUpdateRowEnumerable processUpdateRows)
    {
        Parallel.ForEach(processUpdateRows, p => p.Stop());
    }

    public static async Task Update(this IProcessUpdateRowEnumerable processUpdateRows)
    {
        await Parallel.ForEachAsync(processUpdateRows, async (p, _) => await p.Update());
    }
}