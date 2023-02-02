using JackTheVideoRipper.models;

namespace JackTheVideoRipper.extensions;

public static class ExceptionExtensions
{
    public static void SaveToFile(this Exception exception)
    {
        FileSystem.SerializeToDisk(FileSystem.GetDownloadPath($"stacktrace_{DateTime.Now:yyyyMMddHHmmss}"), 
            new ExceptionModel(exception));
    }

    public static Type GetBaseType(this Exception exception)
    {
        return exception.GetBaseException().GetType();
    }
    
    public static string GetBaseTypeName(this Exception exception)
    {
        return exception.GetBaseType().ToString();
    }

    public static string GetCaller(this Exception exception)
    {
        return nameof(exception.TargetSite);
    }
}