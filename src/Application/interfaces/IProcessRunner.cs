using System.Diagnostics;
using JackTheVideoRipper.models;
using JackTheVideoRipper.models.enums;
using JackTheVideoRipper.models.processes;

namespace JackTheVideoRipper.interfaces;

public interface IProcessRunner
{
    #region Properties

    Process? Process { get; }
    
    ProcessStatus ProcessStatus { get; }
    
    Guid Guid { get; }
    
    ProcessBuffer Buffer { get; }
    
    MediaProcessType ProcessType { get; init; }

    int ExitCode { get; }
    
    string ProcessFileName { get; }
    
    int ProcessId { get; }
    
    float Progress { get; }
    
    List<string> Dependencies { get; }

    #endregion

    #region Flags

    bool Completed { get; }

    bool Running => Started && !Completed && !Finished;
    
    bool Failed { get; }
    
    bool Succeeded { get; }
    
    bool Started { get; }
                
    bool Finished { get; }
    
    bool Paused { get; }

    #endregion

    #region Methods

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

    #endregion
}