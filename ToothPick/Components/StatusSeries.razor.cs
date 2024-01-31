namespace ToothPick.Components
{
    public partial class StatusSeries : IDisposable
    {
        private bool disposedValue;

        public IEnumerable<KeyValuePair<Series, Status>> Statuses { get; set; } = [];

        [Inject]
        private StatusService StatusService { get; set; } = null!;

        private ModalPrompt ModalPromptReference = null!;
        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                StatusService.UpdateStatusesDelegates.TryAdd(this, UpdateStatuses);
                await UpdateStatuses();
            }

            base.OnAfterRender(firstRender);
        }

        public async Task UpdateStatuses()
        {
            Statuses = [.. StatusService.Statuses];
            await InvokeAsync(StateHasChanged);
        }

        public async Task StopSeries(Status status)
        {
            async void stopSeriesAction()
            {
                status.SerieCancellationTokenSource.Cancel();

                await InvokeAsync(StateHasChanged);
            }

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Cancel series processing?",
                Body = new MarkupString($"<p>Cancel processing the series: {status.Series.Name}? This series will not be processed during this round of processing and will be processed again in the next round.</p>"),
                CancelChoice = "Cancel",
                Choice = "Yes",
                ChoiceColour = "danger",
                ChoiceAction = stopSeriesAction
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StatusService.UpdateStatusesDelegates.TryRemove(this, out _);
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