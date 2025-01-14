﻿using JackTheVideoRipper.extensions;
using Newtonsoft.Json;

namespace JackTheVideoRipper.models
{
    public class AppVersionModel
    {
        [JsonProperty("version")]
        public string VersionString { get; set; } = string.Empty;
        
        [JsonProperty("is_newer_version_available")]
        public bool IsNewerVersionAvailable { get; set; }
        
        [JsonIgnore]
        public Version Version => new(VersionString[1..]);

        [JsonIgnore]
        public bool OutOfDate => Version.CompareTo(LocalVersion) > 0;
        
        private static Version LocalVersion => new(FileSystem.VersionInfo);

        public static async Task<AppVersionModel> GetFromServer()
        {
            return new AppVersionModel
            {
                VersionString = (await GetVersionFromServer())?.Remove("\n").ValueOrDefault()!
            };
        }
        
        private static async Task<string?> GetVersionFromServer()
        {
            HttpResponseMessage response = await FileSystem.SimpleWebQueryAsync(Urls.VersionInfo);

            if (response.IsSuccessStatusCode)
                return await response.GetResponseAsync();
            
            Output.WriteLine(@$"Failed to download {response.ResponseCode()})");
            return null;
        }
    }
}
