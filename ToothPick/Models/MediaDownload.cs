using YoutubeDLSharp.Options;

namespace ToothPick.Models
{
    public class MediaDownload
    {
        public required Media Media { get; set; }
        public required Location Location { get; set; }
        public required OptionSet OptionSet { get; set; }
        public CancellationTokenSource DownloadCancellationTokenSource { get; set; } = new();
    }
}