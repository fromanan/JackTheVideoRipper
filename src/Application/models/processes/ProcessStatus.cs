namespace JackTheVideoRipper.models.processes;

[Flags]
public enum ProcessStatus
{
    // Created, but not queued
    Created = 0,
    
    // Place into work queue
    Queued = 1,
    
    // Currently running
    Running = 1 << 1,

    // Process errored out
    Error = 1 << 2,
    
    // System stopped process
    Stopped = 1 << 3,
    
    // User cancelled process
    Cancelled = 1 << 4,
    
    // Process succeeded
    Succeeded = 1 << 5,
    
    // Process paused
    Paused = 1 << 6,
    
    // Process initializing (pre-run tasks)
    Starting = 1 << 7,
    
    // Process finishing (post-run tasks)
    Finalizing = 1 << 8,
    
    // Process finished (Error, Stopped, Cancelled, Succeeded)
    Completed = Error | Stopped | Cancelled | Succeeded,
    
    // Process failed (Error, Stopped, Cancelled)
    Failed = Error | Stopped | Cancelled,
    
    // Process actively running
    Active = Starting | Running | Finalizing
}