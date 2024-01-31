namespace ToothPick.Models
{
    public static class Defaults
    {
        // Values for defaults and parsing fallback.
        public const int ParallelFetch = 4;
        public const int ParallelDownloads = 4;
        public const int ScanDelayMinutes = 10;
        public const int DefaultFetchCount = 5;
        public const int NewSeriesFetchCountOverride = 5;
        public const int GotifyAppId = -1;
        public const int GotifyLogLevel = 2;

        /// <summary>
        /// Default settings with the key as Name and the value as Value.
        /// </summary>
        public static readonly Dictionary<string, string> Settings = new()
        {
            { "ColorTheme", "Dark" },
            { "DataPath", "/ToothPick/Data" },
            { "DownloadPath", "/ToothPick/Media" },
            { "CookiesPath", "/ToothPick/Cookies" },
            { "GotifyAppId", "" },
            { "GotifyAppToken", "" },
            { "GotifyClientToken", "" },
            { "GotifyHeader", "X-Gotify-Key" },
            { "GotifyLogLevel", "2" },
            { "GotifyUri", "" },
            { "LogFilterTokens", "Incomplete data received;Invalid start time;unavailable videos are hidden;Temporary failure in name resolution;Unable to download JSON metadata" },
            { "DefaultFetchCount", "5" },
            { "NewSeriesFetchCountOverride", "5" },
            { "NewSeriesDownloadEnabledOverride", "False" },
            { "ParallelPlaylistFetch", "4" },
            { "ParallelMediaDataFetch", "4" },
            { "ParallelDownloads","4" },
            { "ScanDelayMinutes", "10" },
            { "ToothPickEnabled", "True" },
            { "DownloadEnabled", "True" },
            { "UserAgent", "Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/115.0"  },
            { "TotalRateLimitMBPS", "" }
        };

        /// <summary>
        /// Default setting descriptions with the key as Name and the value as Description.
        /// </summary>
        public static readonly Dictionary<string, string> SettingDescriptions = new()
        {
            { "ColorTheme", "String - The name of the color theme to use for the ToothPick Web Application UI. Possible values are \"Light\" or \"Dark\". Defaults to \"Dark\"." },
            { "DataPath", "READ-ONLY String - The directory path used to store the database files. Defaults to \"/ToothPick/Data\"." },
            { "CookiesPath", "String - The directory path in which to store and retrieve the cookies used for location-based sessions. Defaults to \"/ToothPick/Data\"." },
            { "DownloadPath", "String - The directory path in which to store downloaded media. Defaults to \"/ToothPick/Data\"." },
            { "GotifyAppId", "Integer - The ID of the Gotify Application in which to post ToothPick notifications." },
            { "GotifyAppToken", "String - The token of the Gotifiy Application in which to post ToothPick notifications." },
            { "GotifyClientToken", "String - The token of the Gotify Client in which to read Toothpick notifications for the main page table." },
            { "GotifyHeader", "String - The header used to authenticate with Gotify, typically \"X-Gotify-Key\"." },
            { "GotifyLogLevel", "Integer - The minimum log level to post to Gotify: Trace = 0, Debug = 1, Information = 2, Warning = 3, Error = 4, Critical = 5, and None = 6. Defaults to 2 (Information)" },
            { "GotifyUri", "String - The Uri for the Gotify instance to use for notifications." },
            { "LogFilterTokens", "String - A case-insensitive, semi-colon delimited list of tokens used to filter out unwanted log messages. Defaults with a couple of commonly ignored warnings: \"Incomplete data received;Invalid start time;unavailable videos are hidden;Temporary failure in name resolution;Unable to download JSON metadata\"" },
            { "DefaultFetchCount", "Integer - The default amount of videos to fetch and check for download if not specified in the location or overrode for a new series. Defaults to 5." },
            { "NewSeriesFetchCountOverride", "Integer - The amount of videos to fetch for a new, no downloaded media, series. Defaults to 5." },
            { "NewSeriesDownloadEnabledOverride", "Boolean - True to enable new media download for a new, no downloaded media, series. False will scan and register media. Registered media will never be downloaded unless their associated entries are deleted." },
            { "ParallelPlaylistFetch", "Integer - The amount of parallel playlist fetches, retrieving media playlist metadata, to perform. Defaults to 4." },
            { "ParallelMediaDataFetch", "Integer - The amount of parallel media data fetches, retrieving specific media metadata, to perform. Defaults to 4." },
            { "ParallelDownloads", "Integer - The amount of parallel downloads to perform. Defaults to 4." },
            { "ScanDelayMinutes", "Integer - The amount of minutes to wait between scanning locations for new media. Defaults to 10." },
            { "ToothPickEnabled", "Boolean - True to enable new media scanning and download, False to disable." },
            { "DownloadEnabled", "Boolean - True to enable new media download, False will scan and register media. Registered media will never be downloaded unless their associated entries are deleted." },
            { "UserAgent", "String - The user agent to use while fetching and downlaoding media. Defaults to \"Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/115.0\""  },
            { "TotalRateLimitMBPS", "Integer - The amount of megabytes per second bandwith allotted to ToothPick for media downloads. Defaults to unlimited." }
        };

        public static readonly List<string> ReadOnlySettings =
        [
            "DataPath"
        ];
    }
}
