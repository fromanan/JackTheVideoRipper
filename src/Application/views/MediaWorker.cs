using System.ComponentModel;
using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.processes;

namespace JackTheVideoRipper.views;

/// <summary>
/// Container class for a Media Task
/// This is NOT updating the external process itself, only checking the output of the Process via the
///     ProcessBuffer (OutputStream) periodically and pushing the updates to the view via the ReportProgress method
/// All actions are controlled by the external process (exe) which we own a handle to, our program can, however,
///     pause/resume or cancel the external process
/// This class is reused by the ProcessPool and may run a task multiple times
/// </summary>
public class MediaWorker : BackgroundWorker, IClaimable
{
    /// <summary>
    /// Sleep delay between update cycles
    /// </summary>
    private const int _DEFAULT_UPDATE_FREQUENCY = 1000;

    private int _updateFrequency;

    /// <summary>
    /// Used in passing the progress back to the ProcessPool since it only uses whole numbers 1-100
    /// </summary>
    public const float PROGRESS_PRECISION_FACTOR = 1E2f;
    
    /// <summary>
    /// Threading property used when our resource is Claimed by a process, but has not started yet (or is paused)
    /// </summary>
    public bool Claimed { get; set; }

    /// <summary>
    /// Called prior to running, but after instantiating the class (we may do that at runtime when the containing class
    ///     is created, if the instance is a property)
    /// </summary>
    public void InitializeWorker(int updateFrequencyInMilliseconds = _DEFAULT_UPDATE_FREQUENCY)
    {
        _updateFrequency = updateFrequencyInMilliseconds;
        WorkerReportsProgress = true;
        WorkerSupportsCancellation = true;
        DoWork += DoMediaWork;
    }
    
    /// <summary>
    /// Method called when the BackgroundWorker.RunAsync() is called
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void DoMediaWork(object? sender, DoWorkEventArgs args)
    {
        if (args.Argument is not ProcessRunner processRunner)
            return;

        while (!CancellationPending && !processRunner.Completed)
        {
            // TODO: Should we only trigger an update when the text changes?
            ProcessUpdateArgs updateArgs = await processRunner.Update();
        
            ReportProgress((int)(processRunner.Progress * PROGRESS_PRECISION_FACTOR), updateArgs);

            Thread.Sleep(_updateFrequency);
        }

        Claimed = false;

        args.Result = ProcessUpdateArgs.Done(processRunner);
    }
}