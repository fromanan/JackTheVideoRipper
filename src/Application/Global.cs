using System.Diagnostics;

namespace JackTheVideoRipper;

internal static class Global
{
    internal static class Fonts
    {
        public const string FAMILY_NAME = "Segoe UI Semibold";

        public static readonly Font Header = new(FAMILY_NAME, 9.5f);
    }

    internal static class Configurations
    {
        public static bool VerboseDebugMode => Debugger.IsAttached;

        public static bool CatchExceptions = true;

        public const int VIEW_UPDATE_TIMEOUT = 500;
        
        public const int WORKER_QUEUE_CHECK_FREQUENCY = 500;

        public const int WORKER_UPDATE_FREQUENCY = 100;
        
        public const int NOTIFICATION_CLEAR_TIME = 5000;
        
        public const int DEFAULT_TASK_TICK = 300;

        public const int PROCESS_POOL_UPDATE_FREQUENCY = 200;
        
        public const int WEB_REQUEST_TIMEOUT_PERIOD = 60000;
    }
}