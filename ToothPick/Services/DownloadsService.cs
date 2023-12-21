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
}
