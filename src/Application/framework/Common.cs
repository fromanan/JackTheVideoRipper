﻿using System.Text.RegularExpressions;
using JackTheVideoRipper.extensions;

namespace JackTheVideoRipper
{
    internal static class Common
    {
        #region Properties

        private static readonly Random _Random = new();

        private static readonly Regex _TitlePattern = new("[^a-zA-Z0-9]", RegexOptions.Compiled);
        
        private static readonly Regex _NumericPattern = new(@"[^\d]", RegexOptions.Compiled);
        
        private static readonly Regex _SpaceSplitPattern = new(@"\s+", RegexOptions.Compiled);
        
        private const string _CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        #endregion

        #region Public Methods

        public static string GetAppVersion()
        {
            return $"v{FileSystem.VersionInfo}";
        }

        public static string FormatTitleForFileName(string title)
        {
            return _TitlePattern.Remove(title).Trim().ValueOrDefault();
        }

        public static string RandomString(int length)
        {
            return Enumerable.Repeat(_CHARACTERS, length).Select(s => s[_Random.Next(s.Length)]).Merge();
        }

        public static string RemoveAllNonNumericValuesFromString(string str)
        {
            return _NumericPattern.Remove(str).ValueOrDefault("0");
        }

        public static void OpenInBrowser(string url)
        {
            if (url.Valid(FileSystem.IsValidUrl))
                Task.Run(() => FileSystem.OpenWebPage(url));
        }

        public static void OpenFileInMediaPlayer(string filepath)
        {
            if (filepath.Valid(File.Exists))
                Task.Run(() => FileSystem.OpenInDefaultProgram(filepath));
        }

        public static void OpenInstallFolder()
        {
            Task.Run(() => FileSystem.OpenFileExplorer(FileSystem.Paths.Install));
        }
        
        public static void RepeatInvoke(Action action, int n, int sleepTime = Global.Configurations.DEFAULT_TASK_TICK)
        {
            for (int i = 0; i < n; i++)
            {
                Application.DoEvents();
                Thread.Sleep(sleepTime);
                action.Invoke();
            }
        }

        public static string[] Tokenize(string line)
        {
            return _SpaceSplitPattern.Split(line);
        }

        public static string CreateTag()
        {
            //return $"{RandomString(5)}{DateTime.UtcNow.Ticks}";
            return Guid.NewGuid().ToString();
        }
        
        public static string TimeString(float timeInSeconds)
        {
            if (float.IsNaN(timeInSeconds) || float.IsInfinity(timeInSeconds))
                return Text.NotApplicable;
            if (timeInSeconds <= 0.001f)
                return Text.DefaultTime;
            double hours = Math.Floor(timeInSeconds / 360);
            double minutes = Math.Floor(timeInSeconds % 360 / 60);
            double seconds = MathF.Floor(timeInSeconds % 60);
            return $"{hours:00.}:{minutes:00.}:{seconds:00.}";
        }

        #endregion
    }
}