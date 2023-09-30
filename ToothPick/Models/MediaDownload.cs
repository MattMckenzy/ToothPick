using YoutubeDLSharp.Options;

namespace ToothPick.Models
{
    public class MediaDownload
    {
        public Media Media { get; set; }
        public OptionSet OptionSet { get; set; }
        public CancellationTokenSource DownloadCancellationTokenSource { get; set; } = new();
    }
}