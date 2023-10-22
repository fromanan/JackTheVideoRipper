namespace JackTheVideoRipper.views;

public class SmoothListView : ListView
{
    private const ControlStyles _DRAW_STYLES = ControlStyles.AllPaintingInWmPaint |
                                               ControlStyles.OptimizedDoubleBuffer;

    public SmoothListView()
    {
        SetStyle(_DRAW_STYLES, true);
    }
}