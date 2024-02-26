using System.Text.Json.Serialization;
using JackTheVideoRipper.framework;
using JackTheVideoRipper.interfaces;

namespace JackTheVideoRipper.models;

using static FileSystem;

[Serializable]
public class ConfigModel : IConfigModel
{
    [JsonIgnore]
    public static readonly string ConfigDirectory = Paths.Config;

    [JsonIgnore]
    public virtual string Filepath { get; } = string.Empty;
    
    [JsonIgnore]
    public bool ExistsOnDisk => File.Exists(Filepath);
    
    private readonly ReaderWriterLockSlim _configLock = new();

    public virtual void WriteToDisk()
    {
        _configLock.EnterReadLock();
        SerializeToDisk(Filepath, this);
        _configLock.ExitReadLock();
    }

    public virtual T? GetFromDisk<T>() where T : ConfigModel
    {
        return GetObjectFromJsonFile<T>(Filepath);
    }

    public virtual T? CreateOrLoadFromDisk<T>() where T : ConfigModel, new()
    {
        if (ExistsOnDisk)
            return GetFromDisk<T>();
        
        CreateFolder(ConfigDirectory);
        WriteToDisk();
        return null;
    }

    public virtual void Validate()
    {
    }
}