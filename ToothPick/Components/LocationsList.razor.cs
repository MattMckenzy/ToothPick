namespace ToothPick.Components
{
    public partial class LocationsList
    {
        #region Services Injection

        [Inject]
        private IDbContextFactory<ToothPickContext> ToothPickContextFactory { get; set; } = null!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        #endregion

        #region Parameters

        [CascadingParameter(Name = nameof(ItemKey))]
        public string? ItemKey { get; set; }

        [CascadingParameter(Name = nameof(Order))]
        public string? Order { get; set; }

        [CascadingParameter(Name = nameof(IsAscending))]
        public string? IsAscendingQuery { get; set; }

        [CascadingParameter(Name = nameof(Filters))]
        public string? FiltersQuery { get; set; }

        #endregion

        #region Private Variables

        private bool IsLoading { get; set; } = true;
        private bool IsUpdating { get; set; } = false;


        private IEnumerable<Library> LibraryCollection { get; set; } = Array.Empty<Library>();
        private IEnumerable<Series> SeriesCollection { get; set; } = Array.Empty<Series>();
        private IEnumerable<Location> LocationsCollection { get; set; } = Array.Empty<Location>();
        private Dictionary<string, string>? LocationsKeys { get; set; }
        private Dictionary<string, IEnumerable<string>>? CategoryKeys { get; set; }

        private Location CurrentLocation { get; set; } = new();

        private string? CurrentFocus { get; set; }
        private string? CurrentFetchCount { get; set; }

        private bool CurrentLocationKeyLocked = false;
        private bool CurrentLocationIsDirty = false;
        private bool CurrentLocationIsValid
        {
            get
            {
                return string.IsNullOrWhiteSpace(LibraryNameFeedback) &&
                    string.IsNullOrWhiteSpace(SeriesNameFeedback) &&
                    string.IsNullOrWhiteSpace(NameFeedback) &&
                    string.IsNullOrWhiteSpace(FetchCountFeedback) &&
                    string.IsNullOrWhiteSpace(UrlFeedback);
            }
        }

        private string LibraryNameFeedback = string.Empty;
        private string SeriesNameFeedback = string.Empty;
        private string NameFeedback = string.Empty;
        private string FetchCountFeedback = string.Empty;
        private string UrlFeedback = string.Empty;

        private ModalPrompt? ModalPromptReference = null;

        private bool IsAscending { get; set; } = true;
        private LocationControlFields SelectedLocationOrder { get; set; } = LocationControlFields.Name;
        private Dictionary<string, string> Filters { get; set; } = [];

        #endregion

        #region Lifecycle Overrides

        protected override Task OnInitializedAsync()
        {
            if (!Filters.ContainsKey(string.Empty))
                Filters.Add(string.Empty, string.Empty);

            foreach (LocationControlFields locationControlFields in Enum.GetValues<LocationControlFields>())
            {
                if (!Filters.ContainsKey(locationControlFields.ToString()))
                    Filters.Add(locationControlFields.ToString(), string.Empty);
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

                SelectedLocationOrder = Enum.TryParse(Order, out LocationControlFields parsedOrder) ? parsedOrder : LocationControlFields.Name;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), SelectedLocationOrder.ToString()), false);

                IsAscending = !bool.TryParse(IsAscendingQuery, out bool parsedIsAscending) || parsedIsAscending;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(IsAscending), IsAscending.ToString()), false);

                if (FiltersQuery != null)
                {
                    foreach (string filterQuery in FiltersQuery.Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    {
                        IEnumerable<string> keyValuePair = filterQuery.Split(":", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        string? key = keyValuePair.ElementAtOrDefault(0);
                        string? value = keyValuePair.ElementAtOrDefault(1);

                        if (key != null && Filters.ContainsKey(key))
                            Filters[key] = value ?? string.Empty;
                    }
                }
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Filters), string.Join(";", Filters.Select(keyValuePair => $"{keyValuePair.Key}:{keyValuePair.Value}"))), false);

                await UpdateLocationsKeys();

                await UpdatePageState();

                if (ItemKey != null && LocationsCollection.Any(location => location.DbKey.ToString().Equals(ItemKey)))
                    await LoadLocation(ItemKey);
                else
                    await CreateLocation();

                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        #endregion

        #region UI Events

        private async Task UpdateProperty(string propertyName, object value)
        {
            switch (propertyName)
            {
                case nameof(Location.FetchCount):
                    CurrentFetchCount = (string?)value;
                    if (int.TryParse((string?)value, out int fetchCountResult))
                    {
                        typeof(Location).GetProperty(propertyName)!.SetValue(CurrentLocation, fetchCountResult);
                        CurrentFetchCount = null;
                    }
                    break;

                default:
                    typeof(Location).GetProperty(propertyName)!.SetValue(CurrentLocation, value);
                    break;
            }

            CurrentFocus = null;

            await UpdatePageState();


            await UpdatePageState();
        }

        private async Task FocusIn(string newFocus)
        {
            CurrentFocus = newFocus;

            await InvokeAsync(StateHasChanged);
        }

        private async Task CreateLocation()
        {
            async void createLocation()
            {
                CurrentLocationKeyLocked = false;

                await SetCurrentLocation(NewLocation());
            }

            if (CurrentLocationIsDirty)
                await ModalPromptReference!.ShowModalPrompt(new()
                {
                    Title = "Discard changes?",
                    Body = new MarkupString($"<p>Create a new location and discard current changes?</p>"),
                    CancelChoice = "Cancel",
                    Choice = "Yes",
                    ChoiceColour = "danger",
                    ChoiceAction = createLocation
                });
            else
                createLocation();
        }

        private async Task LoadLocation(string locationKey)
        {
            if (locationKey.Equals(CurrentLocation.DbKey, StringComparison.InvariantCultureIgnoreCase))
                return;

            Location? location = LocationsCollection.FirstOrDefault(location => location.DbKey.Equals(locationKey, StringComparison.InvariantCultureIgnoreCase));

            if (location == null)
                return;

            async void loadAction()
            {
                CurrentLocationKeyLocked = true;

                await SetCurrentLocation(location);

                await InvokeAsync(StateHasChanged);
            }

            if (CurrentLocationIsDirty)
                await ModalPromptReference!.ShowModalPrompt(new()
                {
                    Title = "Discard Changes?",
                    Body = new MarkupString($"<p>Load the location: \"{location.Name}\" and discard current changes?</p>"),
                    CancelChoice = "Cancel",
                    Choice = "Yes",
                    ChoiceColour = "danger",
                    ChoiceAction = loadAction
                });
            else
                loadAction();
        }

        private async Task SaveLocation()
        {
            if (!CurrentLocationIsValid || !CurrentLocationIsDirty)
                return;

            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            Location? location = (await toothPickContext.Locations.ToArrayAsync()).FirstOrDefault(location => location.DbKey == CurrentLocation.DbKey);

            using ToothPickContext toothPickContext2 = await ToothPickContextFactory.CreateDbContextAsync();
            MarkupString body = default;
            if (location == null)
            {
                body = new($"<p>Succesfully added the new location \"{CurrentLocation.Name}\"!</p>");
                toothPickContext2.Locations.Add(CurrentLocation);
            }
            else
            {
                body = new($"<p>Succesfully updated the location \"{CurrentLocation.Name}\"!</p>");
                toothPickContext2.Locations.Update(CurrentLocation);
            }

            await toothPickContext2.SaveChangesAsync();

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Saved Location",
                Body = body,
                CancelChoice = "Dismiss"
            });

            CurrentLocationKeyLocked = true;

            await UpdatePageState();
        }

        private async Task DeleteLocation(string locationKey)
        {
            Location? location = LocationsCollection.FirstOrDefault(location => location.DbKey.Equals(locationKey, StringComparison.InvariantCultureIgnoreCase));

            if (location == null)
                return;

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Delete the location?",
                Body = new MarkupString($"<p>Really delete the location \"{location.Name}\"?</p>"),
                CancelChoice = "Cancel",
                Choice = "Delete",
                ChoiceColour = "danger",
                ChoiceAction = async () =>
                {
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

                    toothPickContext.Remove(location);
                    await toothPickContext.SaveChangesAsync();

                    await ModalPromptReference.ShowModalPrompt(new()
                    {
                        Title = "Succesfully deleted!",
                        Body = new MarkupString($"<p>Succesfully deleted the location \"{location.Name}\"!</p>"),
                        CancelChoice = "Dismiss"
                    });

                    await UpdatePageState();
                }
            });

        }

        private async Task ToggleOrder()
        {
            IsAscending = !IsAscending;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(IsAscending), IsAscending.ToString()), false);
            await UpdateLocationsKeys();
        }

        private async Task SelectLocationOrder(LocationControlFields locationOrder)
        {
            SelectedLocationOrder = locationOrder;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), locationOrder.ToString()), false);
            await UpdateLocationsKeys();
        }

        private async Task FilterLocations(Dictionary<string, string> filters)
        {
            Filters = filters;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Filters), string.Join(";", Filters.Select(keyValuePair => $"{keyValuePair.Key}:{keyValuePair.Value}"))), false);
            await UpdateLocationsKeys();
        }


        #endregion

        #region Helper Methods

        private Location NewLocation()
        {
            string library = !string.IsNullOrWhiteSpace(Filters["LibraryName"]) &&
                LibraryCollection.Any(library => library.Name == Filters["LibraryName"]) ?
                Filters["LibraryName"] :
                string.Empty;

            string series = !string.IsNullOrWhiteSpace(Filters["SeriesName"]) &&
                SeriesCollection.Any(series => series.Name == Filters["SeriesName"]) ?
                Filters["SeriesName"] :
                string.Empty;

            return new Location() { LibraryName = library, SeriesName = series };
        }

        private async Task UpdatePageState()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            await ValidateCurrentLocation();

            CurrentLocationIsDirty = (!CurrentLocationKeyLocked && !CompareLocations(CurrentLocation, NewLocation())) ||
                (CurrentLocationKeyLocked && !CompareLocations(CurrentLocation, toothPickContext.Locations.ToArray().FirstOrDefault(location => location.DbKey == CurrentLocation.DbKey)));

            LibraryCollection = toothPickContext.Libraries.ToArray();
            SeriesCollection = toothPickContext.Series.ToArray();
            LocationsCollection = toothPickContext.Locations.ToArray();

            await UpdateLocationsKeys();

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task ValidateCurrentLocation()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            // Validate Series.LibraryName if needed.
            LibraryNameFeedback = string.Empty;
            if (!CurrentLocationKeyLocked)
            {
                if (string.IsNullOrWhiteSpace(CurrentLocation.LibraryName))
                    LibraryNameFeedback = "A library name is required!";
                else if (!toothPickContext.Libraries.Any(library => library.Name == CurrentLocation.LibraryName))
                    LibraryNameFeedback = "The library must exist! ";
            }


            // Validate Series.SeriesName if needed.
            SeriesNameFeedback = string.Empty;
            if (!CurrentLocationKeyLocked)
            {
                if (string.IsNullOrWhiteSpace(CurrentLocation.SeriesName))
                    SeriesNameFeedback = "A series name is required!";
                else if (!toothPickContext.Series.Any(series => series.Name == CurrentLocation.SeriesName))
                    SeriesNameFeedback = "The series must exist!";
            }

            // Validate Location.Name
            NameFeedback = string.Empty;
            if (!CurrentLocationKeyLocked)
            {
                if (string.IsNullOrWhiteSpace(CurrentLocation.Name))
                    NameFeedback = "A location name is required!";
                else if (LocationsCollection.FirstOrDefault(location => location.Name == CurrentLocation.Name) != null)
                    NameFeedback = "The location name must be unique! ";
            }

            // Validate Location.Feedback
            FetchCountFeedback = string.Empty;
            if (!string.IsNullOrWhiteSpace(CurrentFetchCount) && !int.TryParse(CurrentFetchCount, out int _))
                FetchCountFeedback = "Fetch Count must be a valid number!";

            // Validate Location.Url
            UrlFeedback = string.Empty;
            if (string.IsNullOrWhiteSpace(CurrentLocation.Url))
                UrlFeedback = "The URL location is required!";
            else if (!string.IsNullOrWhiteSpace(CurrentLocation.Url) &&
                !Uri.IsWellFormedUriString(CurrentLocation.Url, UriKind.Absolute))
                UrlFeedback = "The URL location must be a well-formed absolute URI!";
        }

        private static bool CompareLocations(Location? location1, Location? location2)
        {
            if (location1 == null || location2 == null)
                return false;

            return
                location1.LibraryName.Equals(location2.LibraryName, StringComparison.InvariantCulture) &&
                location1.SeriesName.Equals(location2.SeriesName, StringComparison.InvariantCulture) &&
                location1.Name.Equals(location2.Name, StringComparison.InvariantCulture) &&
                location1.Url.Equals(location2.Url, StringComparison.InvariantCulture) &&
                location1.FetchCount == location2.FetchCount &&
                location1.MatchFilters.Equals(location2.MatchFilters, StringComparison.InvariantCulture) &&
                location1.DownloadFormat.Equals(location2.DownloadFormat, StringComparison.InvariantCulture) &&
                location1.Cookies.Equals(location2.Cookies, StringComparison.InvariantCulture);
        }

        private async Task SetCurrentLocation(Location location)
        {
            CurrentLocation = location;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(ItemKey), location.Name), false);
            await UpdatePageState();
        }

        private async Task UpdateLocationsKeys()
        {
            IsUpdating = true;
            await InvokeAsync(StateHasChanged);

            ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            string globalFilter = Filters[string.Empty];

            LocationsKeys = LocationsCollection.AsQueryable()
                .Where(location =>
                    (
                    location.LibraryName.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    location.SeriesName.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    location.Name.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    location.Url.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    (location.FetchCount.HasValue ? location.FetchCount.Value.ToString() : string.Empty).Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    location.MatchFilters.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    location.DownloadFormat.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    location.Cookies.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase)) &&
                    location.LibraryName.Contains(Filters[LocationControlFields.Name.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    location.SeriesName.Contains(Filters[LocationControlFields.Name.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    location.Name.Contains(Filters[LocationControlFields.Name.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    location.Url.Contains(Filters[LocationControlFields.Url.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    (location.FetchCount.HasValue ? location.FetchCount.Value.ToString() : string.Empty).Contains(Filters[LocationControlFields.FetchCount.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    location.MatchFilters.Contains(Filters[LocationControlFields.MatchFilters.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    location.DownloadFormat.Contains(Filters[LocationControlFields.DownloadFormat.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    location.Cookies.Contains(Filters[LocationControlFields.Cookies.ToString()], StringComparison.CurrentCultureIgnoreCase))
                .OrderBy($"{GetSortingField()} {(IsAscending ? "ASC" : "DESC")}")
                .ToDictionary(location => location.DbKey, location => location.Name);

            CategoryKeys = LocationsCollection.GroupBy(location => $"{location.LibraryName}.{location.SeriesName}").ToDictionary(group => group.Key, group => group.Select(location => location.DbKey));

            IsUpdating = false;
            await InvokeAsync(StateHasChanged);
        }

        private string GetSortingField() => SelectedLocationOrder switch
        {
            LocationControlFields.Cookies => nameof(Location.Cookies),
            LocationControlFields.DownloadFormat => nameof(Location.DownloadFormat),
            LocationControlFields.MatchFilters => nameof(Location.MatchFilters),
            LocationControlFields.FetchCount => nameof(Location.FetchCount),
            LocationControlFields.Url => nameof(Location.Url),
            LocationControlFields.LibraryName => nameof(Location.LibraryName),
            LocationControlFields.SeriesName => nameof(Location.SeriesName),
            LocationControlFields.Name or _ => nameof(Location.Name)
        };

        #endregion
    }
}