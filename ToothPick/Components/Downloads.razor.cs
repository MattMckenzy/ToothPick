namespace ToothPick.Components
{
    public partial class Downloads : IDisposable
    {
        [Parameter]
        public IEnumerable<KeyValuePair<Media, Download>> DownloadList { get; set; } = [];

        [Inject]
        private DownloadsService DownloadsService { get; set; } = null!;

        private bool disposedValue;

        protected override Task OnInitializedAsync()
        {
            DownloadList = DownloadsService.Downloads.ToArray();
            return base.OnInitializedAsync();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                DownloadsService.UpdateDelegates.TryAdd(this, UpdateDownloads);

            base.OnAfterRender(firstRender);
        }

        public async Task UpdateDownloads()
        {
            DownloadList = DownloadsService.Downloads.ToArray();
            await InvokeAsync(StateHasChanged);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DownloadsService.UpdateDelegates.TryRemove(this, out _);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}