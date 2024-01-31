using YoutubeDLSharp;

namespace ToothPick.Models
{
    public class Download
    {
        public required Media Media { get; set; }
        
        public DownloadProgress? DownloadProgress { get; set; } = null;

        public required CancellationTokenSource DownloadCancellationTokenSource { get; set; }

        public ConcurrentDictionary<ComponentBase, Func<DownloadProgress, Task>> UpdateDelegates { get; set; } = new ConcurrentDictionary<ComponentBase, Func<DownloadProgress, Task>>();
    }
}
