﻿namespace ToothPick.Components
{
    public partial class SeriesList
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

        [CascadingParameter(Name = nameof(FromLink))]
        public bool? FromLink { get; set; }

        #endregion

        #region Private Variables

        private bool IsLoading { get; set; } = true;
        private bool IsUpdating { get; set; } = true;

        private IEnumerable<Library> LibraryCollection { get; set; } = [];
        private IEnumerable<Series> SeriesCollection { get; set; } = [];
        private Dictionary<(string SuperCategory, string Category, string Key), string>? SeriesKeys { get; set; }

        private Series CurrentSeries { get; set; } = new() { LibraryName = string.Empty, Name = string.Empty };

        private bool CurrentSeriesKeyLocked = false;
        private bool CurrentSeriesIsDirty = false;
        private bool CurrentSeriesIsValid
        {
            get
            {
                return string.IsNullOrWhiteSpace(LibraryNameFeedback) &&
                    string.IsNullOrWhiteSpace(NameFeedback) &&
                    string.IsNullOrWhiteSpace(ThumbnailLocationFeedback) &&
                    string.IsNullOrWhiteSpace(PosterLocationFeedback) &&
                    string.IsNullOrWhiteSpace(BannerLocationFeedback) &&
                    string.IsNullOrWhiteSpace(LogoLocationFeedback);
            }
        }

        private string LibraryNameFeedback = string.Empty;
        private string NameFeedback = string.Empty;
        private string ThumbnailLocationFeedback = string.Empty;
        private string PosterLocationFeedback = string.Empty;
        private string BannerLocationFeedback = string.Empty;
        private string LogoLocationFeedback = string.Empty;

        private ModalPrompt? ModalPromptReference = null;

        private EntityListLevels EntityListLevel { get; set; } = EntityListLevels.Category;

        private SeriesListLevels SelectedLevel { get; set; } = SeriesListLevels.Library;
        private bool IsAscending { get; set; } = true;
        private SeriesControlFields SelectedSeriesOrder { get; set; } = SeriesControlFields.Name;
        private Dictionary<string, string> Filters { get; set; } = [];

        #endregion

        #region Lifecycle Overrides

        protected override Task OnInitializedAsync()
        {
            Filters.TryAdd(string.Empty, string.Empty);
            foreach (SeriesControlFields seriesControlFields in Enum.GetValues<SeriesControlFields>())
            {
                if (!Filters.ContainsKey(seriesControlFields.ToString()))
                    Filters.Add(seriesControlFields.ToString(), string.Empty);
            }

            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("setIntegerOnly");
                await JSRuntime.InvokeVoidAsync("setNumericOnly");
                await JSRuntime.InvokeVoidAsync("setEnterNext");

                if (!FromLink ?? false)
                {
                    if (string.IsNullOrWhiteSpace(value: Level))
                        Level = await StorageService.Get("SeriesList-Level");

                    if (string.IsNullOrWhiteSpace(Order))
                        Order = await StorageService.Get("SeriesList-Order");

                    if (string.IsNullOrWhiteSpace(IsAscendingQuery))
                        IsAscendingQuery = await StorageService.Get("SeriesList-IsAscending");

                    if (string.IsNullOrWhiteSpace(FiltersQuery))
                        FiltersQuery = await StorageService.Get("SeriesList-Filters");

                    if (string.IsNullOrWhiteSpace(ItemKey))
                        ItemKey = await StorageService.Get("SeriesList-ItemKey");
                }

                SelectedLevel = Enum.TryParse(Level, out SeriesListLevels parsedLevel) ? parsedLevel : SeriesListLevels.Library;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Level), SelectedLevel.ToString()), false);

                SelectedSeriesOrder = Enum.TryParse(Order, out SeriesControlFields parsedOrder) ? parsedOrder : SeriesControlFields.Name;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), SelectedSeriesOrder.ToString()), false);

                IsAscending = !bool.TryParse(IsAscendingQuery, out bool parsedIsAscending) || parsedIsAscending;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(IsAscending), IsAscending.ToString()), false);


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

                if (ItemKey != null)
                {
                    string[] itemKeys = ItemKey.Split("|~|");
                    if (itemKeys.Length == 2)
                        await LoadSeries((string.Empty, itemKeys[0], itemKeys[1]));
                }
                else
                    await CreateSeries();

                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        #endregion

        #region UI Events

        private async Task UpdateProperty(string propertyName, object value)
        {
            typeof(Series).GetProperty(propertyName)!.SetValue(CurrentSeries, value);

            await UpdatePageState();
        }

        private Series NewSeries()
        {
            string library = !string.IsNullOrWhiteSpace(Filters["LibraryName"]) &&
                LibraryCollection.Any(library => library.Name == Filters["LibraryName"]) ?
                Filters["LibraryName"] :
                string.Empty;

            return new Series() { LibraryName = library };
        }

        private async Task CreateSeries()
        {
            async void createSeries()
            {
                CurrentSeriesKeyLocked = false;

                await SetCurrentSeries(NewSeries());
            }

            if (CurrentSeriesIsDirty)
                await ModalPromptReference!.ShowModalPrompt(new()
                {
                    Title = "Discard changes?",
                    Body = new MarkupString($"<p>Create a new series and discard current changes?</p>"),
                    CancelChoice = "Cancel",
                    Choice = "Yes",
                    ChoiceColour = "danger",
                    ChoiceAction = createSeries,
                });
            else
                createSeries();
        }

        private async Task LoadSeries((string _, string Category, string Key) item)
        {
            Series? series = SeriesCollection.FirstOrDefault(series => series.LibraryName == item.Category && series.Name == item.Key);

            if (series == null)
                return;

            async void loadAction()
            {
                CurrentSeriesKeyLocked = true;

                await SetCurrentSeries(series);

                await InvokeAsync(StateHasChanged);
            }

            if (CurrentSeriesIsDirty)
                await ModalPromptReference!.ShowModalPrompt(new()
                {
                    Title = "Discard Changes?",
                    Body = new MarkupString($"<p>Load the series: \"{series.Name}\" and discard current changes?</p>"),
                    CancelChoice = "Cancel",
                    Choice = "Yes",
                    ChoiceColour = "danger",
                    ChoiceAction = loadAction
                });
            else
                loadAction();
        }

        private async Task SaveSeries()
        {
            if (!CurrentSeriesIsValid || !CurrentSeriesIsDirty)
                return;

            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            Series? series = await toothPickContext.Series.FirstOrDefaultAsync(series => CurrentSeries.Name == series.Name);

            using ToothPickContext toothPickContext2 = await ToothPickContextFactory.CreateDbContextAsync();
            MarkupString body = default;
            if (series == null)
            {
                body = new($"<p>Succesfully added the new series \"{CurrentSeries.Name}\"!</p>");
                toothPickContext2.Series.Add(CurrentSeries);
            }
            else
            {
                body = new($"<p>Succesfully updated the series \"{CurrentSeries.Name}\"!</p>");
                toothPickContext2.Series.Update(CurrentSeries);
            }

            await toothPickContext2.SaveChangesAsync();

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Saved Series",
                Body = body,
                CancelChoice = "Dismiss"
            });

            CurrentSeriesKeyLocked = true;

            await UpdatePageState();
        }

        private async Task DeleteSeries(IEnumerable<(string SuperCategory, string Category, string Key)> items)
        {
            IEnumerable<Series> validItems = SeriesCollection.Where(series => items.Any(item => item.Category == series.LibraryName && item.Key == series.Name));

            if (!validItems.Any())
                return;
            else if (validItems.Count() == 1)
            {
                Series series = validItems.First();

                await ModalPromptReference!.ShowModalPrompt(new()
                {
                    Title = "Delete the series?",
                    Body = new MarkupString($"<p>Really delete the series \"{series.Name}\"? This will also delete all child locations and media.</p>"),
                    CancelChoice = "Cancel",
                    Choice = "Delete",
                    ChoiceColour = "danger",
                    ChoiceAction = async () =>
                    {
                        using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

                        toothPickContext.RemoveRange(series);
                        await toothPickContext.SaveChangesAsync();

                        await ModalPromptReference.ShowModalPrompt(new()
                        {
                            Title = "Succesfully deleted!",
                            Body = new MarkupString($"<p>Succesfully deleted the series \"{series.Name}\"!</p>"),
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
                    Title = "Delete all shown series?",
                    Body = new MarkupString($"<p>Really delete the {validItems.Count()} series shown? This will also delete all their child locations and media.</p>"),
                    CancelChoice = "Cancel",
                    Choice = "Delete",
                    ChoiceColour = "danger",
                    ChoiceAction = async () =>
                    {
                        using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

                        toothPickContext.Remove(validItems);
                        await toothPickContext.SaveChangesAsync();

                        await ModalPromptReference.ShowModalPrompt(new()
                        {
                            Title = "Succesfully deleted!",
                            Body = new MarkupString($"<p>Succesfully deleted the {validItems.Count()} series shown!</p>"),
                            CancelChoice = "Dismiss"
                        });

                        await UpdatePageState();
                    }
                });
            }
        }

        private async Task SelectLevel(SeriesListLevels seriesListLevel)
        {
            SelectedLevel = seriesListLevel;
            string seriesLevelQuery = seriesListLevel.ToString();
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Level), seriesLevelQuery), false);

            if (!string.IsNullOrWhiteSpace(seriesLevelQuery))
                await StorageService.Set("SeriesList-Level", seriesLevelQuery);

            await UpdateSeriesKeys();
        }

        private async Task ToggleOrder()
        {
            IsAscending = !IsAscending;
            string isAscendingQuery = IsAscending.ToString();
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(IsAscending), IsAscending.ToString()), false);

            if (!string.IsNullOrWhiteSpace(isAscendingQuery))
                await StorageService.Set("SeriesList-IsAscending", isAscendingQuery);

            await UpdateSeriesKeys();
        }

        private async Task SelectSeriesOrder(SeriesControlFields seriesOrder)
        {
            SelectedSeriesOrder = seriesOrder;
            string seriesOrderQuery = seriesOrder.ToString();
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), seriesOrder.ToString()), false);

            if (!string.IsNullOrWhiteSpace(seriesOrderQuery))
                await StorageService.Set("SeriesList-Order", seriesOrderQuery);

            await UpdateSeriesKeys();
        }

        private async Task FilterSeries(Dictionary<string, string> filters)
        {
            Filters = filters;
            string filtersQuery = string.Join("|~;|", Filters.Select(keyValuePair => $"{keyValuePair.Key}|~:|{keyValuePair.Value}"));
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Filters), filtersQuery), false);
            
            if (!string.IsNullOrWhiteSpace(filtersQuery))
                await StorageService.Set("SeriesList-Filters", filtersQuery);

            await UpdateSeriesKeys();
        }


        #endregion

        #region Helper Methods

        private async Task UpdatePageState()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            await ValidateCurrentSeries();

            CurrentSeriesIsDirty = (!CurrentSeriesKeyLocked && !CompareSeries(CurrentSeries, NewSeries())) ||
                (CurrentSeriesKeyLocked && !CompareSeries(CurrentSeries, toothPickContext.Series.ToArray().FirstOrDefault(series => series.LibraryName == CurrentSeries.LibraryName && series.Name == CurrentSeries.Name)));

            LibraryCollection = [.. toothPickContext.Libraries];
            SeriesCollection = [.. toothPickContext.Series];

            await UpdateSeriesKeys();

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task ValidateCurrentSeries()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            // Validate Series.LibraryName if needed.
            LibraryNameFeedback = string.Empty;
            if (!CurrentSeriesKeyLocked)
            {
                if (string.IsNullOrWhiteSpace(CurrentSeries.LibraryName))
                    LibraryNameFeedback = "A library is required!";
                else if (!toothPickContext.Libraries.Any(library => library.Name == CurrentSeries.LibraryName))
                    LibraryNameFeedback = "The library must exist! ";
            }

            // Validate Series.Name if needed.
            NameFeedback = string.Empty;
            if (!CurrentSeriesKeyLocked)
            {
                if (string.IsNullOrWhiteSpace(CurrentSeries.Name))
                    NameFeedback = "A series name is required!";
                else if (SeriesCollection.FirstOrDefault(series => series.Name == CurrentSeries.Name) != null)
                    NameFeedback = "The series name must be unique! ";
            }

            // Validate Series.ThumbnailLocation
            ThumbnailLocationFeedback = string.Empty;
            if (!string.IsNullOrWhiteSpace(CurrentSeries.ThumbnailLocation) &&
                !Uri.IsWellFormedUriString(CurrentSeries.ThumbnailLocation, UriKind.Absolute))
                ThumbnailLocationFeedback = "The thumbnail location must be a well-formed absolute URI!";

            // Validate Series.ThumbnailLocation
            PosterLocationFeedback = string.Empty;
            if (!string.IsNullOrWhiteSpace(CurrentSeries.PosterLocation) &&
                !Uri.IsWellFormedUriString(uriString: CurrentSeries.PosterLocation, UriKind.Absolute))
                PosterLocationFeedback = "The poster location must be a well-formed absolute URI!";

            // Validate Series.ThumbnailLocation
            BannerLocationFeedback = string.Empty;
            if (!string.IsNullOrWhiteSpace(CurrentSeries.BannerLocation) &&
                !Uri.IsWellFormedUriString(CurrentSeries.BannerLocation, UriKind.Absolute))
                BannerLocationFeedback = "The banner location must be a well-formed absolute URI!";

            // Validate Series.ThumbnailLocation
            LogoLocationFeedback = string.Empty;
            if (!string.IsNullOrWhiteSpace(CurrentSeries.LogoLocation) &&
                !Uri.IsWellFormedUriString(CurrentSeries.LogoLocation, UriKind.Absolute))
                LogoLocationFeedback = "The logo location must be a well-formed absolute URI!";
        }

        private static bool CompareSeries(Series? series1, Series? series2)
        {
            if (series1 == null || series2 == null)
                return false;

            return
                series1.LibraryName.Equals(series2.LibraryName, StringComparison.InvariantCulture) &&
                series1.Name.Equals(series2.Name, StringComparison.InvariantCulture) &&
                series1.Description.Equals(series2.Description, StringComparison.InvariantCulture) &&
                series1.ThumbnailLocation.Equals(series2.ThumbnailLocation, StringComparison.InvariantCulture) &&
                series1.PosterLocation.Equals(series2.PosterLocation, StringComparison.InvariantCulture) &&
                series1.BannerLocation.Equals(series2.BannerLocation, StringComparison.InvariantCulture) &&
                series1.LogoLocation.Equals(series2.LogoLocation, StringComparison.InvariantCulture);
        }

        private async Task SetCurrentSeries(Series series)
        {
            CurrentSeries = series;
            string itemKey = $"{series.LibraryName}|~|{series.Name}";
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(ItemKey), itemKey), false);
            await StorageService.Set("SeriesList-ItemKey", itemKey);

            await UpdatePageState();
        }

        private async Task UpdateSeriesKeys()
        {
            IsUpdating = true;
            await InvokeAsync(StateHasChanged);

            string globalFilter = Filters[string.Empty];

            IQueryable<Series> SeriesKeysQuery = SeriesCollection.AsQueryable()
                .Where(series =>
                    (
                    series.LibraryName.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    series.Name.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    series.Description.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    series.ThumbnailLocation.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    series.BannerLocation.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    series.PosterLocation.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    series.LogoLocation.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase)) &&
                    series.LibraryName.Contains(Filters[SeriesControlFields.LibraryName.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    series.Name.Contains(Filters[SeriesControlFields.Name.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    series.Description.Contains(Filters[SeriesControlFields.Description.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    series.ThumbnailLocation.Contains(Filters[SeriesControlFields.ThumbnailLocation.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    series.BannerLocation.Contains(Filters[SeriesControlFields.BannerLocation.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    series.PosterLocation.Contains(Filters[SeriesControlFields.PosterLocation.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    series.LogoLocation.Contains(Filters[SeriesControlFields.LogoLocation.ToString()], StringComparison.CurrentCultureIgnoreCase));

            if (SelectedLevel == SeriesListLevels.Library)
                SeriesKeysQuery = SeriesKeysQuery.OrderBy($"{nameof(Series.LibraryName)} ASC")
                    .ThenBy($"{GetSortingField()} {(IsAscending ? "ASC" : "DESC")}");
            else
                SeriesKeysQuery = SeriesKeysQuery.OrderBy($"{GetSortingField()} {(IsAscending ? "ASC" : "DESC")}");

            SeriesKeys = SeriesKeysQuery.ToDictionary(series => (string.Empty, series.LibraryName, series.Name), series => series.Name);

            EntityListLevel = GetEntityListLevel();

            IsUpdating = false;
            await InvokeAsync(StateHasChanged);
        }

        private string GetSortingField() => SelectedSeriesOrder switch
        {
            SeriesControlFields.LogoLocation => nameof(Series.LogoLocation),
            SeriesControlFields.PosterLocation => nameof(Series.PosterLocation),
            SeriesControlFields.BannerLocation => nameof(Series.BannerLocation),
            SeriesControlFields.ThumbnailLocation => nameof(Series.ThumbnailLocation),
            SeriesControlFields.Description => nameof(Series.Description),
            SeriesControlFields.LibraryName => nameof(Series.LibraryName),
            SeriesControlFields.Name or _ => nameof(Series.Name)
        };

        private EntityListLevels GetEntityListLevel() => SelectedLevel switch
        {
            SeriesListLevels.Series => EntityListLevels.Entity,
            SeriesListLevels.Library or _ => EntityListLevels.Category,
        };

        #endregion
    }
}