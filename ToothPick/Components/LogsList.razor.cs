using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ToothPick.Components
{
    public partial class LogsList
    {
        [Inject]
        private GotifyService GotifyService { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        private ProtectedLocalStorage ProtectedLocalStorage { get; set; } = null!;

        [CascadingParameter(Name = nameof(Filter))]
        public string? Filter { get; set; }

        private bool IsLoading { get; set; } = true;
        private List<GotifyMessage> GotifyMessages { get; set; } = [];
        private IEnumerable<LogLevel> FilteredLogLevels =
        [
            LogLevel.Information,
            LogLevel.Warning,
            LogLevel.Error,
            LogLevel.Critical
        ];

        protected override async Task OnInitializedAsync()
        {
            await GotifyService.SubscribeToStream(async (gotifyMessage) =>
                {
                    await InvokeAsync(async () => {
                        if (!GotifyMessages.Any(message => message.Id.Equals(gotifyMessage.Id)))
                        {
                            GotifyMessages.Add(gotifyMessage);                                
                            GotifyMessages =
                            [
                                .. GotifyMessages
                                    .Where(message => FilteredLogLevels.Any(logLevel => GotifyService.GetGotifyPriority(logLevel) == message.Priority))
                                    .OrderByDescending(message => message.Date),
                            ];

                            await InvokeAsync(StateHasChanged);
                        }
                    });
                });

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (string.IsNullOrWhiteSpace(Filter))
                {
                    Filter = (await ProtectedLocalStorage.GetAsync<string>("LogsList-Filter")).Value;
                    NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Filter), Filter), false);
                }

                FilteredLogLevels = Filter?.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select<string, LogLevel?>(logLevel => Enum.TryParse(logLevel, out LogLevel parsedLogLevel) ? parsedLogLevel : null)
                .Where(logLevel => logLevel != null)
                .Cast<LogLevel>()
                .ToArray() ?? 
                [
                    LogLevel.Information,
                    LogLevel.Warning,
                    LogLevel.Error,
                    LogLevel.Critical
                ];

                await UpdateLogs();

                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async void LoggingService_LogsChanged(object sender, ChangeEventArgs changeEventArgs)
        {
            await UpdateLogs();
        }

        private async Task UpdateLogs()
        {
            GotifyMessages =
            [
                .. (await GotifyService.GetMessages())
                    .Where(message => FilteredLogLevels.Any(logLevel => GotifyService.GetGotifyPriority(logLevel) == message.Priority))
                    .OrderByDescending(message => message.Date)
            ];

            await InvokeAsync(StateHasChanged);
        }

        private async Task DeleteMessage(GotifyMessage gotifyMessage)
        {
            GotifyMessages.Remove(gotifyMessage);

            if (gotifyMessage.Id != null)
                await GotifyService.DeleteMessage((int)gotifyMessage.Id);            
            else
                await GotifyService.DeleteMessage(gotifyMessage.InternalId);

            await InvokeAsync(StateHasChanged);
        }

        private async Task DeleteAllMessages()
        {
            await GotifyService.DeleteMessages(GotifyService.GetToothPickContextFactory());
            GotifyMessages.Clear();

            await InvokeAsync(StateHasChanged);
        }

        private static string GetMessageStyle(GotifyMessage gotifyMessage)
        {
            return gotifyMessage.Priority switch
            {
                8 or 9 => "list-group-item-danger",
                5 or 6 or 7 => "list-group-item-warning",
                2 or 3 or 4 => "list-group-item-info",
                _ or 0 or 1 => "list-group-item-light"
            };
        }

        private async Task FilterMessages(ChangeEventArgs changeEventArgs)
        {
            if (changeEventArgs?.Value != null && changeEventArgs.Value is IEnumerable<LogLevel> selectedLogLevels)
			{
				FilteredLogLevels = selectedLogLevels;
                string filteredLogLevelsString = string.Join(",", FilteredLogLevels);
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Filter), filteredLogLevelsString), false);

                if (!string.IsNullOrWhiteSpace(filteredLogLevelsString))
                    await ProtectedLocalStorage.SetAsync("LogsList-Filter", filteredLogLevelsString);
                    
                await UpdateLogs();
            }
        }
    }
}
