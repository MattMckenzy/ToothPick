using YoutubeDLSharp;

namespace ToothPick.Services
{
    public class DownloadsService
    {
        public ConcurrentDictionary<Media, Download> Downloads { get; set; }
        
        public ConcurrentDictionary<ComponentBase, Func<Task>> UpdateDelegates { get; set; }

        public DownloadsService()
        {
            Downloads = new ConcurrentDictionary<Media, Download>();
            UpdateDelegates = new ConcurrentDictionary<ComponentBase, Func<Task>>();
        }
    }

    public class Download
    {
        public Media Media { get; set; }

        public ConcurrentDictionary<ComponentBase, Func<DownloadProgress, Task>> UpdateDelegates { get; set; } = new ConcurrentDictionary<ComponentBase, Func<DownloadProgress, Task>>();

        public CancellationTokenSource DownloadCancellationTokenSource { get; set; }
    }
}
