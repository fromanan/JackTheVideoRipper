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
}