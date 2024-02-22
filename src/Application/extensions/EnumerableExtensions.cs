namespace JackTheVideoRipper.extensions;

public static class EnumerableExtensions
{
    public static string Merge<T>(this IEnumerable<T> enumerable, string separator = "")
    {
        return string.Join(separator, enumerable);
    }
    
    public static string Merge<T>(this IEnumerable<T> enumerable, char separator)
    {
        return string.Join(separator, enumerable);
    }
    
    public static string MergeNewline<T>(this IEnumerable<T> enumerable)
    {
        return string.Join("\n", enumerable);
    }
    
    public static string MergeReturn<T>(this IEnumerable<T> enumerable)
    {
        return string.Join("\r\n", enumerable);
    }
    
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (T element in enumerable)
        {
            action(element);
        }
    }

    public static IEnumerable<T> Reversed<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.ToArray().Reverse();
    }

    public static bool All(this IEnumerable<bool> enumerable)
    {
        return enumerable.All(b => b);
    }
    
    public static bool Any(this IEnumerable<bool> enumerable)
    {
        return enumerable.Any(b => b);
    }

    public static bool None<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.Any();
    }

    public static bool Empty<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.Any();
    }

    public static IEnumerable<string> SelectStrings<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Select(e => e?.ToString() ?? "N/A");
    }
}