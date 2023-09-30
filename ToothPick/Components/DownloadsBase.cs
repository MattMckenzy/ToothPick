namespace ToothPick.Components
{
    public class DownloadsBase : ComponentBase, IDisposable
    {
        [Parameter]
        public IEnumerable<KeyValuePair<Media, Download>> Downloads { get; set; }

        [Inject]
        private DownloadsService DownloadsService { get; set; }

        private bool disposedValue;

        protected override Task OnInitializedAsync()
        {
            Downloads = DownloadsService.Downloads.ToArray();
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
            Downloads = DownloadsService.Downloads.ToArray();
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