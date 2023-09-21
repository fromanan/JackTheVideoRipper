namespace JackTheVideoRipper.models;

public class DeveloperException : Exception
{
    public DeveloperException()
    {
    }
    
    public DeveloperException(string message) : base(message)
    {
    }
    
    public DeveloperException(string message, Exception innerException) : base(message, innerException)
    {
    }
}