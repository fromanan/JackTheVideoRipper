using System.Diagnostics;
using JackTheVideoRipper.models;
using JackTheVideoRipper.models.processes;

namespace JackTheVideoRipper.interfaces;

public interface IProcessRunner
{
    Process? Process { get; }
    
    ProcessStatus ProcessStatus { get; }
    
    Guid Guid { get; }
    
    ProcessBuffer Buffer { get; }

    int ExitCode { get; }

    bool Completed { get; }

    bool Running => Started && !Completed;
    
    string ProcessFileName { get; }
    
    bool Failed { get; }
    
    bool Succeeded { get; }
    
    bool Started { get; }
                
    bool Finished { get; }
    
    bool Paused { get; }
    
    int ProcessId { get; }
    
    List<string> Dependencies { get; }

    Task<ProcessUpdateArgs> Update();

    Task<bool> Start();

    void Stop();

    void Retry();

    void Cancel();

    void Pause();

    void Resume();

    void Enqueue();

    void Kill();
    
    void TryKillProcess();
}