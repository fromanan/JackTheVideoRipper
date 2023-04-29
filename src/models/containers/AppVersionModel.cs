﻿using JackTheVideoRipper.extensions;
using Newtonsoft.Json;

namespace JackTheVideoRipper.models
{
    public class AppVersionModel
    {
        private const string _ASSEMBLY_INFO_FILE_URL =
            "https://raw.githubusercontent.com/fromanan/JackTheVideoRipper/master/version";
        
        [JsonProperty("version")]
        public string VersionString { get; set; } = string.Empty;
        
        [JsonProperty("is_newer_version_available")]
        public bool IsNewerVersionAvailable { get; set; }
        
        [JsonIgnore]
        public Version Version => new(VersionString[1..]);

        [JsonIgnore]
        public bool OutOfDate => Version.CompareTo(LocalVersion) > 0;
        
        private static Version LocalVersion => new(FileSystem.VersionInfo);

        public static AppVersionModel GetFromServer()
        {
            return new AppVersionModel
            {
                VersionString = GetVersionFromServer()?.Remove("\n") ?? string.Empty
            };
        }
        
        private static string? GetVersionFromServer()
        {
            HttpResponseMessage response = FileSystem.SimpleWebQuery(_ASSEMBLY_INFO_FILE_URL);

            if (response.IsSuccessStatusCode)
            {
                return response.GetResponse();
            }
            
            Console.WriteLine(@$"Failed to download {response.ResponseCode()})");
            return null;
        }
    }
}
