using Microsoft.JSInterop;

namespace ToothPick.Components
{
    public partial class SettingsList
    {
        #region Services Injection

        [Inject]
        private IDbContextFactory<ToothPickContext> ToothPickContextFactory { get; set; } = null!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        private ProtectedLocalStorage ProtectedLocalStorage { get; set; } = null!;

        #endregion

        #region Parameters

        [CascadingParameter(Name = nameof(ItemKey))]
        public string? ItemKey { get; set; }

        #endregion

        #region Private Variables

        private bool IsLoading { get; set; } = true;

        private IEnumerable<Setting> Settings { get; set; } = Array.Empty<Setting>();
        private Dictionary<(string SuperCategory, string Category, string Key), string>? SettingKeys { get; set; }

        private Setting? CurrentSetting { get; set; } = null;
        private bool CurrentSettingIsValid = false;
        private bool CurrentSettingIsDirty = false;

        private ModalPrompt ModalPromptReference = null!;

        #endregion

        #region Lifecycle Overrides

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("setEnterNext");
                await UpdatePageState();
                
                if (string.IsNullOrWhiteSpace(ItemKey))
                    ItemKey = (await ProtectedLocalStorage.GetAsync<string>("SettingsList-ItemKey")).Value;

                if (ItemKey != null && Settings.Any(setting => setting.Name.Equals(ItemKey)))
                    await LoadSetting((string.Empty, string.Empty, ItemKey));
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        #endregion

        #region UI Events

        private async Task UpdateProperty(string propertyName, object value)
        {
            typeof(Setting).GetProperty(propertyName)!.SetValue(CurrentSetting, value);

            await UpdatePageState();
        }

        private async Task LoadSetting((string _, string __, string Key) item)
        {
            Setting? setting = Settings.FirstOrDefault(setting => setting.Name == item.Key);

            if (setting == null || setting.Name == CurrentSetting?.Name)
                return;

            async void loadAction()
            {
                using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

                await SetCurrentSetting((Setting)toothPickContext.Entry(setting).CurrentValues.Clone().ToObject());

                await InvokeAsync(StateHasChanged);
            }

            if (CurrentSettingIsDirty)
                await ModalPromptReference!.ShowModalPrompt(new()
                {
                    Title = "Discard Changes?",
                    Body = new MarkupString($"<p>Load the setting: \"{setting.Name}\" and discard current changes?</p>"),
                    CancelChoice = "Cancel",
                    Choice = "Yes",
                    ChoiceColour = "danger",
                    ChoiceAction = loadAction
                });
            else
                loadAction();
        }

        private async Task SaveSetting()
        {
            if (!CurrentSettingIsValid || !CurrentSettingIsDirty || CurrentSetting == null)
                return;

            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();
                
            await toothPickContext.Settings.SetSettingAsync(CurrentSetting.Name, CurrentSetting.Value);

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Settings",
                Body = new MarkupString($"<p>Saved the setting \"{CurrentSetting.Name}\"!</p>"),
                CancelChoice = "Dismiss"
            });

            await UpdatePageState();
        }

        private async Task DeleteSetting((string _, string __, string Key) item)
        {
            Setting? setting = Settings.FirstOrDefault(setting => setting.Name.Equals(item.Key, StringComparison.InvariantCultureIgnoreCase));

            if (setting == null)
                return;

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Delete the setting?",
                Body = new MarkupString($"<p>Really delete the setting \"{setting.Name}\"?</p>"),
                CancelChoice = "Cancel",
                Choice = "Delete",
                ChoiceColour = "danger",
                ChoiceAction = async () =>
                {
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

                    toothPickContext.Remove(setting);
                    await toothPickContext.SaveChangesAsync();

                    await ModalPromptReference.ShowModalPrompt(new()
                    {
                        Title = "Succesfully deleted!",
                        Body = new MarkupString($"<p>Succesfully deleted the setting \"{setting.Name}\"!</p>"),
                        CancelChoice = "Dismiss"
                    });

                    await UpdatePageState();
                }
            });

        }

        #endregion

        #region Helper Methods

        private async Task UpdatePageState()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            CurrentSettingIsValid =
                !string.IsNullOrWhiteSpace(CurrentSetting?.Name) &&
                !Defaults.ReadOnlySettings.Contains(CurrentSetting?.Name ?? string.Empty);

            CurrentSettingIsDirty = !string.IsNullOrWhiteSpace(CurrentSetting?.Name) && !CompareSettings(CurrentSetting, toothPickContext.Settings.ToArray().FirstOrDefault(setting => setting.Name.Equals(CurrentSetting.Name)));

            Settings = toothPickContext.Settings.ToArray();
            SettingKeys = Settings
                .OrderBy(setting => setting.Name)
                .ToDictionary(setting => (string.Empty, string.Empty, setting.Name), setting => setting.Name);
            
            IsLoading = false;

            await InvokeAsync(StateHasChanged);
        }

        private static bool CompareSettings(Setting? setting1, Setting? setting2)
        {
            if (setting1 == null || setting2 == null)
                return false;

            return setting1.Name.Equals(setting2.Name, StringComparison.InvariantCulture) &&
                setting1.Value.Equals(setting2.Value, StringComparison.InvariantCulture);
        }

        private async Task SetCurrentSetting(Setting setting)
        {
            CurrentSetting = setting;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(ItemKey), setting.Name), false);

            await ProtectedLocalStorage.SetAsync("SettingsList-ItemKey", setting.Name);

            await UpdatePageState();
        }

        #endregion
    }
}