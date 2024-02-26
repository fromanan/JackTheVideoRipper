using Newtonsoft.Json;

namespace JackTheVideoRipper.models.containers;

internal class PlaylistInfoItem
{
    [JsonProperty("id")]
    public string? Id { get; set; }
        
    [JsonProperty("title")]
    public string? Title { get; set; }
}