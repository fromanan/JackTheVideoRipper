using JackTheVideoRipper.extensions;
using JackTheVideoRipper.models.enums;

namespace JackTheVideoRipper.models.processes;

public record ProcessResult(int ExitCode = -1, string Output = "")
{
    public ProcessResult(ExitCode exitCode, string output = "") : this(exitCode.Cast<int>(), output)
    {
    }
    
    public readonly int ExitCode = ExitCode;
    public readonly string Output = Output;

    public bool Failed => ExitCode > 0;

    public bool Succeeded => ExitCode is 0;

    public bool Invalid => ExitCode < 0;

    public static readonly ProcessResult Timeout = new(enums.ExitCode.TimeoutExceeded);
    
    public static readonly ProcessResult FailureGeneric = new(enums.ExitCode.Generic);
}