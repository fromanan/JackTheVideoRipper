using JackTheVideoRipper.models.rows;

namespace JackTheVideoRipper.models.containers;

public struct Notification
{
    public DateTime? DateQueued = null;
    public DateTime? DatePosted = null;
    public readonly string Message;
    public readonly string? ShortenedMessage;
    public readonly string SenderName;
    public readonly Guid SenderGuid;

    public NotificationRow? NotificationRow = null;
    
    public string[] ViewItemArray => new[] {DateQueued.ToString()!, SenderName, SenderGuid.ToString(), Message};

    public Notification(string message, Type type, string? shortenedMessage = null)
    {
        Message = message;
        SenderName = type.Name;
        SenderGuid = type.GUID;
        ShortenedMessage = shortenedMessage;
    }
    
    public Notification(string message, object sender, string? shortenedMessage = null)
    {
        Message = message;
        SenderName = sender.GetType().Name;
        SenderGuid = sender.GetType().GUID;
        ShortenedMessage = shortenedMessage;
    }
}