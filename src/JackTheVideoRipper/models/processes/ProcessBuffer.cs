using System.Diagnostics;
using JackTheVideoRipper.extensions;

namespace JackTheVideoRipper.models;

public class ProcessBuffer
{
    #region Data Members

    protected readonly ProcessLog Log = new();
    
    public List<string> Results { get; private set; } = new() { string.Empty }; // Placeholder result
    
    public int Cursor { get; set; } // where in message buffer are we
    
    private int _lineCount;
    
    public event Action<ProcessLogNode> LogAdded = delegate {  };

    public int ResultCount
    {
        get;
        private set;
    }
    
    public int ErrorCount
    {
        get;
        private set;
    }

    #endregion

    #region Attributes

    public bool AtEndOfBuffer => Cursor >= Results.Count - 1;
    
    public string ProcessLine => Cursor < Results.Count ? Results[Cursor] : string.Empty;
    
    public string[] TokenizedProcessLine => ProcessLine.HasValue() ? 
        Common.Tokenize(ProcessLine) : 
        Array.Empty<string>();

    #endregion

    #region Public Methods

    public void Initialize(Process process)
    {
        Log.AddProcessInfo(process);
        process.OutputDataReceived += (_, args) => AppendResult(args.Data);
        process.ErrorDataReceived += (_, args) => AppendError(args.Data);
        process.EnableRaisingEvents = true;
    }

    public void Update()
    {
        // No new updates to process (or ship to the view)
        if (AtEndOfBuffer)
            return;

        Cursor++;
    }
    
    public void SkipToEnd()
    {
        if (Cursor + 10 < Results.Count)
            Cursor = Results.Count;
    }
    
    public IEnumerable<string> GetLogMessages() => Log.Messages;
    
    public IEnumerable<ProcessLogNode> GetLogNodes() => Log.Logs;

    public void Clear()
    {
        Log.Clear();
        Results = new List<string> { string.Empty };
        Cursor = 0;
        ResultCount = 0;
        ErrorCount = 0;
        _lineCount = 0;
    }

    public void SaveLogs()
    {
        FileSystem.SerializeAndDownload(Log);
    }
    
    public string GetError()
    {
        return GetAfterHeader("ERROR: ");
    }

    public string GetAfterHeader(string header)
    {
        return GetResultWhere(r => r.StartsWith(header))?.After(header).ValueOrDefault()!;
    }

    public string? GetResultWhere(Func<string,bool> predicate)
    {
        return Results.FirstOrDefault(predicate);
    }

    public string? GetResultWhichContains(string str)
    {
        return GetResultWhere(r => r.Contains(str));
    }

    public bool Contains(string str, StringComparison stringComparison = StringComparison.Ordinal)
    {
        return Results.Any(line => line.Contains(str, stringComparison));
    }

    #endregion

    #region Private Methods
    
    private void AppendResult(string? line)
    {
        if (line != null && line.HasValue())
        {
            AddResultLine(line, ProcessLogType.Log);
            ResultCount++;
        }
    }

    private void AppendError(string? line)
    {
        if (line != null && line.HasValue())
        {
            AddResultLine(line, ProcessLogType.Error);
            ErrorCount++;
        }
    }

    public void AddLog(string message, ProcessLogType logType)
    {
        AddNonResultLine(message, logType);
    }

    private void AddResultLine(string message, ProcessLogType processLogType)
    {
        Results.Add(message);
        AddNonResultLine(message, processLogType);
    }
    
    private void AddNonResultLine(string message, ProcessLogType processLogType)
    {
        ProcessLogNode logNode = new(processLogType, message, DateTime.Now);
        Log.Add(logNode);
        LogAdded(logNode);
        //Log.Add($"{_resultCount} > {body}");
        _lineCount++;
    }

    #endregion
}