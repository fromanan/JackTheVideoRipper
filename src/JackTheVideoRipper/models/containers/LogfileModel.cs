using Newtonsoft.Json;

namespace JackTheVideoRipper.models.containers;

[Serializable]
public class LogfileModel
{
    [JsonProperty("log_nodes")]
    public List<LogNode> LogNodes = new();

    public void Add(LogNode logNode)
    {
        LogNodes.Add(logNode);
    }
}