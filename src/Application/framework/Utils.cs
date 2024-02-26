namespace JackTheVideoRipper.framework;

public static class Utils
{
    public static string FormatSize(string size)
    {
        if (size.Contains("KiB"))
            return size.Replace("KiB", " KB");
        if (size.Contains("MiB"))
            return size.Replace("MiB", " MB");
        if (size.Contains("GiB"))
            return size.Replace("GiB", " GB");
        if (size.Contains("TeB"))
            return size.Replace("TeB", " TB");
        return size;
    }
}