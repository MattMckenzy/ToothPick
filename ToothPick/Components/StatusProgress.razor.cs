namespace ToothPick.Components
{
    public partial class StatusProgress : IDisposable
    {
        protected bool Enabled { get; set; }
        protected bool NoLocations { get; set; }
        protected int Progress { get; set; }
        protected int TotalProcessingSeries { get; set; }
        protected int ProgressPercent { get; set; }
        protected DateTime? NextProcessingTime { get; set; }
        protected TimeSpan TimeUntilNextProcessing { get; set; } = TimeSpan.Zero;

        protected CancellationTokenSource? ProcessingCancellationTokenSource { get; set; }

        [Inject]
        private IDbContextFactory<ToothPickContext> ToothPickContextFactory { get; set; } = null!;

        [Inject]
        private StatusService StatusService { get; set; } = null!;

        private PeriodicTimer PeriodicTimer { get; } = new(TimeSpan.FromSeconds(1));

        private ModalPrompt ModalPromptReference = null!;

        private bool disposedValue;

        protected override async Task OnInitializedAsync()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();
            bool toothPickEnabled = !bool.TryParse((await toothPickContext.Settings.GetSettingAsync("ToothPickEnabled")).Value, out bool parsedToothPickEnabled) || parsedToothPickEnabled;

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
            Enabled = !Enabled;
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();
            await toothPickContext.Settings.SetSettingAsync("ToothPickEnabled", Enabled.ToString());
        }

        public async Task StopWaiting()
        {
            async void stopWaitAction()
            {
                ProcessingCancellationTokenSource?.Cancel();

                await InvokeAsync(StateHasChanged);
            }

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Stop the wait?",
                Body = new MarkupString($"<p>Stop the wait and begin processing immediately? The next round of processing will immediately begin.</p>"),
                CancelChoice = "Cancel",
                Choice = "Yes",
                ChoiceColour = "danger",
                ChoiceAction = stopWaitAction
            });
        }

        public async Task StopProcessing()
        {
            async void stopProcessingAction()
            {
                ProcessingCancellationTokenSource?.Cancel();

                await InvokeAsync(StateHasChanged);
            }

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Stop processing?",
                Body = new MarkupString($"<p>Cancel all processing activities? This round of processing will be canceled and will resume after the the configured wait time.</p>"),
                CancelChoice = "Cancel",
                Choice = "Yes",
                ChoiceColour = "danger",
                ChoiceAction = stopProcessingAction
            });
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