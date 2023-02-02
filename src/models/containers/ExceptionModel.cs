using System.Text;
using JackTheVideoRipper.extensions;
using Newtonsoft.Json;

namespace JackTheVideoRipper.models;

[Serializable]
public class ExceptionModel
{
    [JsonProperty("message")]
    public string Message;
    
    [JsonProperty("source")]
    public string? Source;
    
    [JsonProperty("caller")]
    public string Caller;
    
    [JsonProperty("type")]
    public string Type;
    
    [JsonProperty("stack_trace")]
    public string? StackTrace;

    [JsonConstructor]
    public ExceptionModel(string message, string? source, string caller, string type, string? stackTrace)
    {
        Message = message;
        Source = source;
        Caller = caller;
        Type = type;
        StackTrace = stackTrace;
    }
    
    public ExceptionModel(string message, string? source, string caller, Type type, string? stackTrace)
    {
        Message = message;
        Source = source;
        Caller = caller;
        Type = type.ToString();
        StackTrace = stackTrace;
    }

    public ExceptionModel(Exception exception)
    {
        Message = exception.Message;
        Source = exception.Source;
        Caller = exception.GetCaller();
        Type = exception.GetBaseTypeName();
        StackTrace = exception.StackTrace;
    }
    
    public ExceptionModel(Exception exception, Type type)
    {
        Message = exception.Message;
        Source = exception.Source;
        Caller = exception.GetCaller();
        Type = type.ToString();
        StackTrace = exception.StackTrace;
    }

    public override string ToString()
    {
        StringBuilder buffer = new();

        buffer.AppendLine($"Exception Type: {Type}");
        buffer.AppendLine($"Caller: {Caller}");
        buffer.AppendLine($"Message: {Message}");
        buffer.AppendLine($"Source: {Source}");
        buffer.AppendLine("Stack Trace:");
        buffer.AppendLine(StackTrace);
        
        return buffer.ToString();
    }
}