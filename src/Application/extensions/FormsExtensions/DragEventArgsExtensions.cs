namespace JackTheVideoRipper.extensions;

public static class DragEventArgsExtensions
{
    public static bool IsText(this DragEventArgs args)
    {
        return args.Data?.GetDataPresent(DataFormats.Text) ?? false;
    }

    public static object? AsText(this DragEventArgs args)
    {
        return args.Data?.GetData(DataFormats.Text);
    }
    
    public static bool IsFile(this DragEventArgs args)
    {
        return args.Data?.GetDataPresent(DataFormats.FileDrop) ?? false;
    }

    public static object? AsFile(this DragEventArgs args)
    {
        return args.Data?.GetData(DataFormats.FileDrop);
    }
    
    public static bool IsValidDroppable(this DragEventArgs args)
    {
        return args.IsText() || args.IsFile();
    }
}