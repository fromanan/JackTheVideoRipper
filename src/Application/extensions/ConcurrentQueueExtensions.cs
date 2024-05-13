using System.Collections.Concurrent;

namespace JackTheVideoRipper.extensions;

public static class ConcurrentQueueExtensions
{
    public static void Extend<T>(this ConcurrentQueue<T> queue, IEnumerable<T> enumerable)
    {
        enumerable.ForEach(queue.Enqueue);
    }
}