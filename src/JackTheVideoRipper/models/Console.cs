using JackTheVideoRipper.extensions;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.views;

namespace JackTheVideoRipper.models;

public class Console : IDisposable
{
    #region Data Members

    private FrameConsole? _frameConsole;

    public readonly string InstanceName = string.Empty;
    
    public ConsoleControl.ConsoleControl? Control { get; private set; }

    public bool Paused { get; private set; }
    
    private readonly List<ILogNode> _logHistory = new();
    
    private readonly Queue<ILogNode> _messageQueue = new();

    #endregion

    #region Attributes
    
    public bool Active => Visible && !Paused;

    private bool Visible => _frameConsole?.Visible ?? false;

    #endregion

    #region Constructor

    public Console(string instanceName)
    {
        InstanceName = instanceName;
    }
    
    public Console()
    {
    }

    #endregion

    #region Public Methods

    public async Task Open(string? instanceName = null)
    {
        if (_frameConsole is not null && Visible)
        {
            await _frameConsole.MoveToTop();
            return;
        }

        await InitializeFrame(instanceName).ContinueWith(async _ =>
        {
            await InitializeMessageQueue();
        });
    }

    public void WriteOutput(ILogNode logNode)
    {
        QueueLog(logNode);
        WriteFromQueue();
    }

    public async Task LockOutput(Task task)
    {
        Task.Run(PauseQueue).Start();
        await task;
        UnpauseQueue();
    }

    public void PauseQueue()
    {
        Paused = true;
    }

    public void UnpauseQueue()
    {
        Paused = false;
        WriteFromQueue();
    }

    #endregion

    #region Private Methods
    
    private void QueueLog(ILogNode logNode)
    {
        _logHistory.Add(logNode);
        _messageQueue.Enqueue(logNode);
    }
    
    private async Task InitializeFrame(string? instanceName = null)
    {
        _frameConsole = new FrameConsole(instanceName ?? InstanceName);
        _frameConsole.FreezeConsoleEvent += PauseQueue;
        _frameConsole.UnfreezeConsoleEvent += UnpauseQueue;
        Control = _frameConsole.ConsoleControl;
        await _frameConsole.OpenConsole();
    }

    private async Task InitializeMessageQueue()
    {
        _messageQueue.Clear();
        _messageQueue.Extend(_logHistory);
        await Tasks.WaitUntil(() => Active);
        WriteFromQueue();
    }

    private void WriteFromQueue()
    {
        while (Active && !_messageQueue.Empty() && Control is not null)
        {
            Control?.WriteLog(_messageQueue.Dequeue());
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _frameConsole?.Dispose();
    }

    #endregion
}