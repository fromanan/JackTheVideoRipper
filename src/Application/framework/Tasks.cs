namespace JackTheVideoRipper;

public static class Tasks
{
    public static async Task WaitUntil(Func<bool> predicate, int tickInMilliseconds = Global.Configurations.DEFAULT_TASK_TICK)
    {
        while (!predicate())
        {
            Application.DoEvents();
            await Task.Delay(tickInMilliseconds);
        }
    }

    public static async Task StartAfter(Action action, int tickInMilliseconds = Global.Configurations.DEFAULT_TASK_TICK)
    {
        await Task.Delay(tickInMilliseconds);
        await Threading.RunInMainContext(action);
    }

    public static async Task<T> YieldAfter<T>(Func<T> func, int delayInMilliseconds)
    {
        await Task.Delay(delayInMilliseconds);
        return func.Invoke();
    }
}