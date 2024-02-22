namespace JackTheVideoRipper.interfaces;

public interface IProcessUpdateRow : IProcessRunner, IListViewItemRow
{
    public string Title
    {
        get;
        set;
    }
        
    public string Status
    {
        get;
        set;
    }

    public string MediaType
    {
        get;
        set;
    }
        
    public string FileSize
    {
        get;
        set;
    }
        
    public new string Progress
    {
        get;
        set;
    }
        
    public string Speed
    {
        get;
        set;
    }
        
    public string Eta
    {
        get;
        set;
    }
        
    public string Url
    {
        get;
        set;
    }
        
    public string Path
    {
        get;
        set;
    }
        
    private Color Color
    {
        get => ViewItem.BackColor;
        set => ViewItem.BackColor = value;
    }

    Task OpenInConsole();

    void SaveLogs();

    void Detach();
    
    int CompareProgress(string progress);
}