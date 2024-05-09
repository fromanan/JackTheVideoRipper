using System.Diagnostics;
using System.Runtime.InteropServices;
using JackTheVideoRipper.framework;
using JackTheVideoRipper.models;

namespace JackTheVideoRipper.extensions;

public static class ProcessExtensions
{
    #region Properties

    private const string _SUSPEND = "suspend";
    
    private const string _RESUME = "resume";

    private static readonly Dictionary<string, Func<IntPtr, int>> _MethodDict = new()
    {
        { _SUSPEND, SuspendThread },
        { _RESUME, ResumeThread }
    };

    #endregion
    
    #region Embedded Types
    
    // https://learn.microsoft.com/en-us/windows/win32/procthread/thread-security-and-access-rights
    
    [Flags]
    private enum ThreadAccess : ulong
    {
        // Required to terminate a thread using TerminateThread.
        TERMINATE                   = 0x0001,
        
        // Required to suspend or resume a thread (see SuspendThread and ResumeThread).
        SUSPEND_RESUME              = 0x0002,
        
        // Required to read the context of a thread using GetThreadContext.
        GET_CONTEXT                 = 0x0008,
        
        // Required to write the context of a thread using SetThreadContext.
        SET_CONTEXT                 = 0x0010,
        
        // Required to set certain information in the thread object.
        SET_INFORMATION             = 0x0020,
        
        // Required to read certain information from the thread object, such as the exit code (see GetExitCodeThread).
        QUERY_INFORMATION           = 0x0040,
        
        // Required to set the impersonation token for a thread using SetThreadToken.
        SET_THREAD_TOKEN            = 0x0080,
        
        // Required to use a thread's security information directly without calling it by using a communication mechanism that provides impersonation services.
        IMPERSONATE                 = 0x0100,
        
        // Required for a server thread that impersonates a client.
        DIRECT_IMPERSONATION        = 0x0200,
        
        // Required to set certain information in the thread object. A handle that has the SET_INFORMATION access right is automatically granted SET_LIMITED_INFORMATION.Windows Server 2003 and Windows XP: This access right is not supported.
        SET_LIMITED_INFORMATION     = 0x0400,
        
        // Required to read certain information from the thread objects (see GetProcessIdOfThread). A handle that has the QUERY_INFORMATION access right is automatically granted QUERY_LIMITED_INFORMATION.Windows Server 2003 and Windows XP: This access right is not supported.
        QUERY_LIMITED_INFORMATION   = 0x0800,
        
        // Required to delete the object.
        DELETE                      = 0x00010000L,
        
        // Required to read information in the security descriptor for the object, not including the information in the SACL. To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. For more information, see SACL Access Right.
        READ_CONTROL                = 0x00020000L,
        
        // Required to modify the DACL in the security descriptor for the object.
        WRITE_DACL                  = 0x00040000L,
        
        // Required to change the owner in the security descriptor for the object.
        WRITE_OWNER                 = 0x00080000L,
        
        // Enables the use of the thread handle in any of the wait functions. / The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.
        SYNCHRONIZE                 = 0x00100000L,
        
        // All possible access rights for a thread object.Windows Server 2003 and Windows XP: The value of the ALL_ACCESS flag increased on Windows Server 2008 and Windows Vista. If an application compiled for Windows Server 2008 and Windows Vista is run on Windows Server 2003 or Windows XP, the ALL_ACCESS flag contains access bits that are not supported and the function specifying this flag fails with ERROR_ACCESS_DENIED. To avoid this problem, specify the minimum set of access rights required for the operation. If ALL_ACCESS must be used, set _WIN32_WINNT to the minimum operating system targeted by your application (for example, #define _WIN32_WINNT _WIN32_WINNT_WINXP). For more information, see Using the Windows Headers.
        //ALL_ACCESS,
    }

    #endregion

    #region Imports
    
    // IntPtr => void* exposed as an int

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, ulong dwThreadId);
    
    [DllImport("kernel32.dll")]
    private static extern int SuspendThread(IntPtr hThread);
    
    [DllImport("kernel32.dll")]
    private static extern int ResumeThread(IntPtr hThread);
    
    [DllImport("kernel32.dll")]
    private static extern int ExitProcess(uint uExitCode);
    
    [DllImport("kernel32.dll")]
    private static extern int TerminateProcess(IntPtr hProcess, uint uExitCode);
    
    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(ThreadAccess dwDesiredAccess, bool bInheritHandle, ulong dwThreadId);
    
    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);

    #endregion

    #region Public Methods
    
    public static int Exit(this Process process, uint exitCode)
    {
        return ExitProcess(exitCode);
    }

    public static int Terminate(this Process process, uint exitCode)
    {
        return TerminateProcess(new IntPtr(process.Id), exitCode);
    }

    public static void Suspend(this Process process)
    {
        foreach (uint threadId in process.GetThreadIds())
        {
            ThreadOperation(threadId, _SUSPEND, ThreadAccess.SUSPEND_RESUME);
        }
    }
    
    public static void Resume(this Process process)
    {
        foreach (uint threadId in process.GetThreadIds())
        {
            ThreadOperation(threadId, _RESUME, ThreadAccess.SUSPEND_RESUME);
        }
    }

    public static string GetOutput(this Process process)
    {
        return process.StandardOutput.ReadToEnd().Trim();
    }
    
    public static async Task<string> GetOutputAsync(this Process process)
    {
        return (await process.StandardOutput.ReadToEndAsync()).Trim();
    }

    public static string GetProcessInfo(this Process process)
    {
        ProcessStartInfo processStartInfo = process.StartInfo;
        return new []
        {
            $"Program: {Path.GetFileName(processStartInfo.FileName).WrapQuotes()}",
            $" > Path: {processStartInfo.FileName.WrapQuotes()}",
            $" > Arguments: {processStartInfo.Arguments}",
            $" > Working Directory: {processStartInfo.WorkingDirectory.WrapQuotes()}",
        }.MergeNewline();
    }
    
    public static Task WaitForExitAsync(this Process process, CancellationToken? cancellationToken = null)
    {
        if (process.HasExited)
            return Task.CompletedTask;

        TaskCompletionSource<object?> completionSource = new();

        process.EnableRaisingEvents = true;
        process.Exited += OnProcessExited;
        cancellationToken?.Register(OnProcessCancelled);

        return process.HasExited ? Task.CompletedTask : completionSource.Task;

        void OnProcessCancelled()
        {
            completionSource.SetCanceled(cancellationToken.Value);
        }

        void OnProcessExited(object? sender, EventArgs eventArgs)
        {
            completionSource.TrySetResult(null);
        }
    }

    public static IEnumerable<ProcessThread> GetThreads(this Process process)
    {
        try
        {
            return !process.HasExited ?
                Enumerable.Cast<ProcessThread>(process.Threads) :
                Enumerable.Empty<ProcessThread>();
        }
        catch (SystemException exception)
        {
            Output.LogException(exception);
            return Enumerable.Empty<ProcessThread>();
        }
    }

    private static ProcessThread[] ToArray(this ProcessThreadCollection threads)
    {
        ProcessThread[] array = new ProcessThread[threads.Count];
        threads.CopyTo(array, 0);
        return array;
    }

    public static IEnumerable<int> GetThreadIds(this Process process)
    {
        return process.GetThreads().Select(t => t.Id);
    }
    
    #endregion

    #region Private Methods
    
    private static IntPtr GetProcessThread(ulong id, ThreadAccess threadAccess)
    {
        try
        {
            return OpenThread(threadAccess, false, id);
        }
        catch (Exception exception)
        {
            Output.LogException(exception);
            return IntPtr.Zero;
        }
    }

    private static void ThreadOperation(ulong threadId, string operation, ThreadAccess threadAccess)
    {
        IntPtr openThread = GetProcessThread(threadId, threadAccess);
        if (openThread == IntPtr.Zero)
            return;
        
        int result;
        try
        {
            result = _MethodDict[operation].Invoke(openThread);
        }
        catch (Exception exception)
        {
            throw new DeveloperException($"Failed to {operation} thread (id = {threadId})!", exception);
        }
        finally
        {
            CloseHandle(openThread);
        }
        
        if (result is -1)
        {
            throw new DeveloperException($"Failed to {operation} thread (id = {threadId}) -- (error code: {Marshal.GetLastWin32Error()})");
        }
    }

    #endregion
}