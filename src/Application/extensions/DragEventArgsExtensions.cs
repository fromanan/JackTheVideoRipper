namespace JackTheVideoRipper.extensions;

public static class DragEventArgsExtensions
{
    public static bool IsText(this DragEventArgs e)
    {
        return e.Data?.GetDataPresent(DataFormats.Text) ?? false;
    }

    public static object? AsText(this DragEventArgs e)
    {
        return e.Data?.GetData(DataFormats.Text);
    }
    
    public static bool IsFile(this DragEventArgs e)
    {
        return e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false;
    }

    public static object? AsFile(this DragEventArgs e)
    {
        return e.Data?.GetData(DataFormats.FileDrop);
    }
    
    public static bool IsValidDroppable(this DragEventArgs e)
    {
        return e.IsText() || e.IsFile();
    }
}