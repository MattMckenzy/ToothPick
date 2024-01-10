using YoutubeDLSharp;

namespace ToothPick.Components
{
    public partial class DownloadItem : IDisposable
    {

        protected DownloadProgress? DownloadProgress = null;
        protected int progress = 0;

        [Inject]
        private IDbContextFactory<ToothPickContext> ToothPickContextFactory { get; set; } = null!;

        [Parameter]
        public required Media Media { get; set; }

        [Parameter]
        public required Download Download { get; set; }

        [Parameter]
        public int Index { get; set; }

        private ModalPrompt ModalPromptReference = null!;

        private bool disposedValue;

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

        public async Task CancelDownload(Download download)
        {
            async void stopDownloadAction()
            {
                download.DownloadCancellationTokenSource.Cancel();
                await InvokeAsync(StateHasChanged);
            }

            async void saveTrackingAction()
            {
                download.DownloadCancellationTokenSource.Cancel();

                using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

                if ((await toothPickContext.FindAsync<Media>(download.Media.LibraryName, download.Media.SeriesName, download.Media.Url)) == null)
                {
                    toothPickContext.Media.Add(download.Media);
                    await toothPickContext.SaveChangesAsync();
                }

                await InvokeAsync(StateHasChanged);
            }

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Cancel download?",
                Body = new MarkupString($"<p>Stop media download? The download of the media, {(string.IsNullOrWhiteSpace(download.Media.Title) ? download.Media.Url : download.Media.Title)}, will be immediately stopped. You may also choose to save the media item so that it won't download again.</p>"),
                CancelChoice = "Cancel",
                Choice = "Stop",
                ChoiceColour = "danger",
                ChoiceAction = stopDownloadAction,
                OtherChoice = "Stop and Save",
                OtherChoiceAction = saveTrackingAction,
                OtherChoiceColour = "danger"
            });
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