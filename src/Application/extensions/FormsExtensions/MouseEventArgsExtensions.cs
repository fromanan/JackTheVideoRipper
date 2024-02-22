namespace JackTheVideoRipper.extensions;

public static class MouseEventArgsExtensions
{
    public static bool IsRightClick(this MouseEventArgs args)
    {
        return args.Button is MouseButtons.Right;
    }
    
    public static bool IsLeftClick(this MouseEventArgs args)
    {
        return args.Button is MouseButtons.Left;
    }
    
    public static bool IsMiddleClick(this MouseEventArgs args)
    {
        return args.Button is MouseButtons.Middle;
    }
}