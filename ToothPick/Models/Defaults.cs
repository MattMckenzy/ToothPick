namespace ToothPick.Models
{
    public static class Defaults
    {
        // Values for defaults and parsing fallback.
        public const int ParallelFetch = 4;
        public const int ParallelDownloads = 4;
        public const int ScanDelayMinutes = 10;
        public const int NewSeriesFetchCountOverride = 10;
        public const int GotifyAppId = -1;
        public const int GotifyLogLevel = 2;

        /// <summary>
        /// Default settings with the key as Name and Service and the value as Description and Value.
        /// </summary>
        public static readonly Dictionary<string, string> Settings = new()
        {
            { "CookiesPath", "/ToothPick/Cookies" },
            { "DownloadPath", "/ToothPick/Media" },
            { "GotifyAppId", "" },
            { "GotifyAppToken", "" },
            { "GotifyClientToken", "" },
            { "GotifyHeader", "X-Gotify-Key" },
            { "GotifyLogLevel", "2" },
            { "GotifyUri", "" },
            { "NewSeriesFetchCountOverride", "1" },
            { "ParallelDownloads","4" },
            { "ParallelFetch", "4" },
            { "ScanDelayMinutes", "10" },
            { "ToothPickEnabled", "True" },
            { "UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36"  },
            { "TotalRateLimitMBPS", "" }
        };
    }
}
