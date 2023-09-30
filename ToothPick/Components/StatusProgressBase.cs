namespace ToothPick.Components
{
    public class StatusProgressBase : ComponentBase, IDisposable
    {
        protected Setting EnabledSetting { get; set; }
        protected bool Enabled { get; set; }
        protected bool NoLocations { get; set; }
        protected int Progress { get; set; }
        protected int TotalProcessingSeries { get; set; }
        protected int ProgressPercent { get; set; }
        protected DateTime? NextProcessingTime { get; set; }
        protected TimeSpan TimeUntilNextProcessing { get; set; } = TimeSpan.Zero;

        protected CancellationTokenSource ProcessingCancellationTokenSource { get; set; }

        [Inject]
        private IDbContextFactory<ToothPickContext> ToothPickContextFactory { get; set; } = null!;

        [Inject]
        private StatusService StatusService { get; set; }
        
        [CascadingParameter(Name = "StopProcessingCallback")]
        public EventCallback<CancellationTokenSource> StopProcessingCallback { get; set; }

        [CascadingParameter(Name = "StopWaitingCallback")]
        public EventCallback<CancellationTokenSource> StopWaitingCallback { get; set; }

        private PeriodicTimer PeriodicTimer { get; } = new(TimeSpan.FromSeconds(1));

        private bool disposedValue;

        protected override async Task OnInitializedAsync()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();
            EnabledSetting = await toothPickContext.Settings.GetSettingAsync("ToothPickEnabled");
            bool toothPickEnabled = !bool.TryParse(EnabledSetting.Value, out bool parsedToothPickEnabled) || parsedToothPickEnabled;

            Enabled = toothPickEnabled;
            
            NoLocations = !toothPickContext.Locations.Any();
            
            NextProcessingTime = StatusService.NextProcessingTime;
            
            _ = Task.Run(async () => {
                while(!disposedValue){
                    await PeriodicTimer.WaitForNextTickAsync();
                    if (NextProcessingTime != null)
                    {
                        TimeUntilNextProcessing = (NextProcessingTime - DateTime.Now.ToLocalTime()).Value;
                        await InvokeAsync(StateHasChanged);
                    }
                }
            });

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                StatusService.UpdateProcessingDelegates.TryAdd(this, UpdateProgress);
                await UpdateProgress();
            }

            base.OnAfterRender(firstRender);
        }

        public async Task UpdateProgress()
        {
            Progress = StatusService.ProcessingPercent;
            TotalProcessingSeries = StatusService.TotalProcessingSeries;
            NextProcessingTime = StatusService.NextProcessingTime;

            if (TotalProcessingSeries > 0)
                ProgressPercent = Progress * 100 / TotalProcessingSeries;
            
            else            
                ProgressPercent = 0;
            
            ProcessingCancellationTokenSource = StatusService.ProcessingCancellationTokenSource;
            await InvokeAsync(StateHasChanged);            
        }

        protected async void ToggleEnabled()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();
            Enabled = !Enabled;
            EnabledSetting.Value = Enabled.ToString();
            toothPickContext.Update(EnabledSetting);
            await toothPickContext.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StatusService.UpdateProcessingDelegates.TryRemove(this, out _);
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