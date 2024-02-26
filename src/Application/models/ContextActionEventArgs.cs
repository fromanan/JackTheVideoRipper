using JackTheVideoRipper.models.enums;

namespace JackTheVideoRipper.models;

public class ContextActionEventArgs : EventArgs
{
    public readonly ContextActions ContextAction;
            
    public ContextActionEventArgs(ContextActions contextAction)
    {
        ContextAction = contextAction;
    }
}