using JackTheVideoRipper.interfaces;
using Newtonsoft.Json;

namespace JackTheVideoRipper.models;

[Serializable]
public readonly struct LogNode : ILogNode
{
    [JsonProperty("date_time")]
    public readonly DateTime DateTime;
    
    [JsonProperty("message")]
    public readonly string Message;
    
    [JsonProperty("color")]
    public readonly Color Color;

    [JsonConstructor]
    public LogNode(DateTime dateTime, string message, Color color)
    {
        DateTime = dateTime;
        Message = message;
        Color = color;
    }

    #region ILogNode Implementation

    public IReadOnlyList<ConsoleLine> Serialize()
    {
        return new[] { new ConsoleLine($"[{DateTime:G}]: {Message}") { Color = Color } };
    }

    #endregion
}