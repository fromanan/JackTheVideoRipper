using JackTheVideoRipper.extensions;

namespace JackTheVideoRipper.models;

public readonly struct FfmpegFrame
{
    public readonly int Frame;
    public readonly float Fps;
    public readonly float Q;
    public readonly long Size;
    public readonly TimeOnly Time;
    public readonly float Bitrate;
    public readonly string BitrateUnit;
    public readonly float Speed;

    public FfmpegFrame(IReadOnlyList<string> tokens)
    {
        int count = 0;
        Frame = int.TryParse(GetField(tokens, ref count), out int frame) ? frame : -1;
        Fps = float.TryParse(GetField(tokens, ref count), out float fps) ? fps : -1;
        Q = float.TryParse(GetField(tokens, ref count), out float q) ? q : -1;
        Size = long.TryParse(GetField(tokens, ref count).BeforeFirstLetter(), out long size) ? size * 1000 : -1;
        Time = TimeOnly.TryParse(GetField(tokens, ref count), out TimeOnly time) ? time : default;

        string bitrateField = GetField(tokens, ref count);
        Bitrate = float.TryParse(bitrateField.BeforeFirstLetter(), out float bitrate) ? bitrate : -1;
        BitrateUnit = bitrateField.AfterFirstLetter();
            
        Speed = float.TryParse(GetField(tokens, ref count), out float speed) ? speed : -1;
    }
    
    private static string GetField(IReadOnlyList<string> tokens, ref int count)
    {
        string value = tokens[count++];
        if (value.ContainsNumber())
            return value.Split('=')[1];
        if (count >= tokens.Count)
            return string.Empty;
        return tokens[count++];
    }
}