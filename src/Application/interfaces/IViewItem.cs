namespace JackTheVideoRipper.interfaces;

public interface IViewItem
{
    public string Tag { get; }
    
    public IViewSubItemCollection SubItems { get; }
    
    public IViewSubItem this[int index] { get; }
    
    Color BackColor { get; set; }
    
    int ImageIndex { get; set; }

    void Suspend();

    void Resume();
}