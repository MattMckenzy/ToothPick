using YoutubeDLSharp;

namespace ToothPick.Components
{
    public class DownloadItemBase : ComponentBase, IDisposable
    {
        private bool disposedValue;

        protected DownloadProgress DownloadProgress = null;
        protected int progress = 0;

        [Parameter]
        public Media Media { get; set; }

        [Parameter]
        public Download Download { get; set; }

        [Parameter]
        public int Index { get; set; }

        [CascadingParameter]
        public EventCallback<Download> SaveTrackingCallback { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                Download.UpdateDelegates.TryAdd(this, UpdateProgress);

            base.OnAfterRender(firstRender);
        }

        public async Task UpdateProgress(DownloadProgress downloadProgress)
        {
            DownloadProgress = downloadProgress;
            progress = Convert.ToInt32((downloadProgress?.Progress ?? 0) * 100);
            await InvokeAsync(StateHasChanged);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Download.UpdateDelegates.TryRemove(this, out _);
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