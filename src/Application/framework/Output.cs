using System.Diagnostics;
using JackTheVideoRipper.models.containers;
using JackTheVideoRipper.viewmodels;
using Console = JackTheVideoRipper.models.Console;

namespace JackTheVideoRipper.framework;

internal static class Output
{
    #region Data Members

    private static readonly Console _Console = new("Main");
    
    public static bool ConsoleAttached { get; private set; }

    private static readonly LogfileModel _LogfileModel = new();

    private static readonly TextWriter _StandardOut = System.Console.Out;

    #endregion

    #region Properties

    private static bool OutputAvailable => _Console.Active && ConsoleAttached;

    private static Type CallerType => new StackTrace().GetFrame(2)?.GetMethod()?.DeclaringType!;

    #endregion

    #region Writing Methods

    public static void Write(string message, Color? color = null, bool sendAsNotification = false)
    {
        if (Global.Configurations.VerboseDebugMode)
            _StandardOut.Write(message);
        
        if (sendAsNotification)
            NotificationsManager.SendNotification(new Notification(message, CallerType));

        LogNode logNode = CreateLog(message, color);
        _LogfileModel.Add(logNode);
        _Console.WriteOutput(logNode);
    }
    
    public static void Write(object? message, Color? color = null, bool sendAsNotification = false)
    {
        if (message is null)
            return;

        Write(message.ToString()!, color, sendAsNotification);
    }

    public static void WriteData(object sender, DataReceivedEventArgs args)
    {
        if (args.Data is not null)
            WriteLine(args.Data);
    }
    
    public static void WriteLine(string message, Color? color = null, bool sendAsNotification = false)
    {
        Write($"{message}\n", color, sendAsNotification);
    }
    
    public static void WriteLine(object? message, Color? color = null, bool sendAsNotification = false)
    {
        if (message is null)
            return;
        
        Write($"{message}\n", color, sendAsNotification);
    }

    public static void LogException<T>(T exception) where T : Exception
    {
        WriteLine($"Exception Thrown:\n{new ExceptionModel(exception, typeof(T))}");
    }

    public static void SaveLogs()
    {
        FileSystem.SerializeToDisk(FileSystem.MergePaths(FileSystem.Paths.Logs,
            $"{FileSystem.TimeStampDate}.logfile"), _LogfileModel);
    }

    #endregion

    #region Public Methods

    public static async Task OpenMainConsoleWindow()
    {
        await _Console.Open();

        if (!ConsoleAttached)
            ConsoleAttached = Input.OpenConsole();
    }
    
    #endregion

    #region Private Methods

    private static LogNode CreateLog(string message, Color? color = null)
    {
        return new LogNode(DateTime.Now, message, color ?? Color.White);
    }

    #endregion
}