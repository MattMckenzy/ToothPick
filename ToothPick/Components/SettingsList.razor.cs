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
        private StorageService StorageService { get; set; } = null!;

        #endregion

        #region Parameters

        [CascadingParameter(Name = nameof(ItemKey))]
        public string? ItemKey { get; set; }

        [CascadingParameter(Name = nameof(Level))]
        public string? Level { get; set; }

        [CascadingParameter(Name = nameof(Order))]
        public string? Order { get; set; }

        [CascadingParameter(Name = nameof(IsAscending))]
        public string? IsAscendingQuery { get; set; }

        [CascadingParameter(Name = nameof(Filters))]
        public string? FiltersQuery { get; set; }

        #endregion


        #region Private Variables

        private bool IsLoading { get; set; } = true;
        private bool IsUpdating { get; set; } = true;

        private IEnumerable<Setting> Settings { get; set; } = [];
        private Dictionary<(string SuperCategory, string Category, string Key), string>? SettingKeys { get; set; }

        private Setting? CurrentSetting { get; set; } = null;
        private bool CurrentSettingIsValid = false;
        private bool CurrentSettingIsDirty = false;

        private ModalPrompt ModalPromptReference = null!;

        private SettingsListLevels SelectedLevel { get; set; } = SettingsListLevels.Settings;
        private bool IsAscending { get; set; } = true;
        private SettingsControlFields SelectedSettingsOrder { get; set; } = SettingsControlFields.Name;
        private Dictionary<string, string> Filters { get; set; } = [];

        #endregion

        #region Lifecycle Overrides

        protected override Task OnInitializedAsync()
        {
            Filters.TryAdd(string.Empty, string.Empty);
            foreach (SettingsControlFields settingsControlFields in Enum.GetValues<SettingsControlFields>())
            {
                if (!Filters.ContainsKey(settingsControlFields.ToString()))
                    Filters.Add(settingsControlFields.ToString(), string.Empty);
            }

            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("setEnterNext");
                
                if (string.IsNullOrWhiteSpace(value: Level))
                    Level = await StorageService.Get("SettingsList-Level");

                SelectedLevel = Enum.TryParse(Level, result: out SettingsListLevels parsedLevel) ? parsedLevel : SettingsListLevels.Settings;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Level), SelectedLevel.ToString()), false);
            
                if (string.IsNullOrWhiteSpace(Order))
                    Order = await StorageService.Get("SettingsList-Order");

                SelectedSettingsOrder = Enum.TryParse(Order, out SettingsControlFields parsedOrder) ? parsedOrder : SettingsControlFields.Name;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), SelectedSettingsOrder.ToString()), false);

                if (string.IsNullOrWhiteSpace(IsAscendingQuery))
                    IsAscendingQuery = await StorageService.Get("SettingsList-IsAscending");

                IsAscending = bool.TryParse(IsAscendingQuery, out bool parsedIsAscending) && parsedIsAscending;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(IsAscending), IsAscending.ToString()), false);

                if (string.IsNullOrWhiteSpace(FiltersQuery))
                    FiltersQuery = await StorageService.Get("SettingsList-Filters");

                if (FiltersQuery != null)
                {
                    foreach (string filterQuery in FiltersQuery.Split("|~;|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    {
                        IEnumerable<string> keyValuePair = filterQuery.Split("|~:|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        string? key = keyValuePair.ElementAtOrDefault(0);
                        string? value = keyValuePair.ElementAtOrDefault(1);

                        if (key != null && Filters.ContainsKey(key))
                            Filters[key] = value ?? string.Empty;
                    }
                }
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Filters), string.Join("|~;|", Filters.Select(keyValuePair => $"{keyValuePair.Key}|~:|{keyValuePair.Value}"))), false);
    
                await UpdatePageState();
                
                if (string.IsNullOrWhiteSpace(ItemKey))
                    ItemKey = await StorageService.Get("SettingsList-ItemKey");

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

        private async Task DeleteSettings(IEnumerable<(string SuperCategory, string Category, string Key)> items)
        {
            IEnumerable<Setting> validItems = Settings.Where(setting => items.Any(item => item.Key == setting.Name));

            if (!validItems.Any())
                return;
            else if (validItems.Count() == 1)
            {
                Setting setting = validItems.First();

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
            else
            {
                await ModalPromptReference!.ShowModalPrompt(new()
                {
                    Title = "Delete all shown settings?",
                    Body = new MarkupString($"<p>Really delete the {validItems.Count()} settings shown?"),
                    CancelChoice = "Cancel",
                    Choice = "Delete",
                    ChoiceColour = "danger",
                    ChoiceAction = async () =>
                    {
                        using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

                        toothPickContext.RemoveRange(validItems);
                        await toothPickContext.SaveChangesAsync();

                        await ModalPromptReference.ShowModalPrompt(new()
                        {
                            Title = "Succesfully deleted!",
                            Body = new MarkupString($"<p>Succesfully deleted the {validItems.Count()} settings shown!</p>"),
                            CancelChoice = "Dismiss"
                        });

                        await UpdatePageState();
                    }
                });
            }
        }

        private async Task SelectLevel(SettingsListLevels settingListLevels)
        {
            SelectedLevel = settingListLevels;
            string settingLevelQuery = settingListLevels.ToString();
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Level), settingLevelQuery), false);
        
            if (!string.IsNullOrWhiteSpace(settingLevelQuery))
                await StorageService.Set("SettingsList-Level", settingLevelQuery);

            await UpdateSettingsKeys();
        }


        private async Task ToggleOrder()
        {
            IsAscending = !IsAscending;
            string isAscendingQuery = IsAscending.ToString();
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(IsAscending), IsAscending.ToString()), false);

            if (!string.IsNullOrWhiteSpace(isAscendingQuery))
                await StorageService.Set("SettingsList-IsAscending", isAscendingQuery);

            await UpdateSettingsKeys();
        }

        private async Task SelectSettingsOrder(SettingsControlFields settingsOrder)
        {
            SelectedSettingsOrder = settingsOrder;
            string settingsOrderQuery = settingsOrder.ToString();
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), settingsOrder.ToString()), false);

            if (!string.IsNullOrWhiteSpace(settingsOrderQuery))
                await StorageService.Set("SettingsList-Order", settingsOrderQuery);

            await UpdateSettingsKeys();
        }

        private async Task FilterSettings(Dictionary<string, string> filters)
        {
            Filters = filters;
            string filtersQuery = string.Join("|~;|", Filters.Select(keyValuePair => $"{keyValuePair.Key}|~:|{keyValuePair.Value}"));
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Filters), filtersQuery), false);
            
            if (!string.IsNullOrWhiteSpace(filtersQuery))
                await StorageService.Set("SettingsList-Filters", filtersQuery);

            await UpdateSettingsKeys();
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

            Settings = [.. toothPickContext.Settings];
            
            await UpdateSettingsKeys();

            IsLoading = false;

            await InvokeAsync(StateHasChanged);
        }

        private async Task UpdateSettingsKeys()
        {
            IsUpdating = true;
            await InvokeAsync(StateHasChanged);

            string globalFilter = Filters[string.Empty];

            SettingKeys = Settings.AsQueryable()
                .Where(settings =>
                    (
                    settings.Name.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    settings.Value.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    settings.Description.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase)) &&
                    settings.Name.Contains(Filters[LibraryControlFields.Name.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    settings.Value.Contains(Filters[SeriesControlFields.Description.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    settings.Description.Contains(Filters[SeriesControlFields.Description.ToString()], StringComparison.CurrentCultureIgnoreCase))
                .OrderBy($"{GetSortingField()} {(IsAscending ? "ASC" : "DESC")}")
                .ToDictionary(setting => (string.Empty, string.Empty, setting.Name), setting => setting.Name);

            IsUpdating = false;
            await InvokeAsync(StateHasChanged);
        }

        private static bool CompareSettings(Setting? setting1, Setting? setting2)
        {
            if (setting1 == null || setting2 == null)
                return false;

            return setting1.Name.Equals(setting2.Name, StringComparison.InvariantCulture) &&                
                setting1.Value.Equals(setting2.Value, StringComparison.InvariantCulture) &&
                setting1.Description.Equals(setting2.Description, StringComparison.InvariantCulture);
        }

        private async Task SetCurrentSetting(Setting setting)
        {
            CurrentSetting = setting;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(ItemKey), setting.Name), false);

            await StorageService.Set("SettingsList-ItemKey", setting.Name);

            await UpdatePageState();
        }

        private string GetSortingField() => SelectedSettingsOrder switch
        {
            SettingsControlFields.Value => nameof(Setting.Value),
            SettingsControlFields.Description => nameof(Setting.Description),
            SettingsControlFields.Name or _ => nameof(Setting.Name)
        };

        #endregion
    }
}