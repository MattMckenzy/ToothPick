namespace ToothPick.Components
{
    public class StatusSeriesBase : ComponentBase, IDisposable
    {
        private bool disposedValue;

        [Parameter]
        public Serie Serie { get; set; }

        [Parameter]
        public IEnumerable<KeyValuePair<Serie, Status>> Statuses { get; set; }

        [CascadingParameter]
        public EventCallback<Status> StopSeriesCallback { get; set; }

        [Inject]
        private StatusService StatusService { get; set; }

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
            Statuses = StatusService.Statuses.ToArray();
            await InvokeAsync(StateHasChanged);
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