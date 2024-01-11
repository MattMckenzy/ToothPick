namespace ToothPick.Components
{
    public partial class MediaList
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

        private IEnumerable<Media> MediaCollection { get; set; } = Array.Empty<Media>();
        private Dictionary<(string SuperCategory, string Category, string Key), string>? MediaKeys { get; set; }

        private Media CurrentMedia { get; set; } = new();

        private ModalPrompt? ModalPromptReference = null;

        private bool IsAscending { get; set; } = true;
        private MediaControlFields SelectedMediaOrder { get; set; } = MediaControlFields.Title;
        private Dictionary<string, string> Filters { get; set; } = [];

        #endregion

        #region Lifecycle Overrides

        protected override Task OnInitializedAsync()
        {
            if (!Filters.ContainsKey(string.Empty))
                Filters.Add(string.Empty, string.Empty);

            foreach (MediaControlFields mediaControlFields in Enum.GetValues<MediaControlFields>())
            {
                if (!Filters.ContainsKey(mediaControlFields.ToString()))
                    Filters.Add(mediaControlFields.ToString(), string.Empty);
            }

            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (string.IsNullOrWhiteSpace(Order))
                    Order = (await ProtectedLocalStorage.GetAsync<string>("MediaList-Order")).Value;

                SelectedMediaOrder = Enum.TryParse(Order, out MediaControlFields parsedOrder) ? parsedOrder : MediaControlFields.DatePublished;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), SelectedMediaOrder.ToString()), false);

                if (string.IsNullOrWhiteSpace(IsAscendingQuery))
                    IsAscendingQuery = (await ProtectedLocalStorage.GetAsync<string>("MediaList-IsAscending")).Value;

                IsAscending = bool.TryParse(IsAscendingQuery, out bool parsedIsAscending) && parsedIsAscending;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(IsAscending), IsAscending.ToString()), false);

                if (string.IsNullOrWhiteSpace(FiltersQuery))
                    FiltersQuery = (await ProtectedLocalStorage.GetAsync<string>("MediaList-Filters")).Value;

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

                await UpdateMediaKeys();

                await UpdatePageState();

                if (string.IsNullOrWhiteSpace(ItemKey))
                    ItemKey = (await ProtectedLocalStorage.GetAsync<string>("MediaList-ItemKey")).Value;

                if (ItemKey != null)
                {
                    string[] itemKeys = ItemKey.Split("|~|");
                    if (itemKeys.Length == 3)
                        await LoadMedia((itemKeys[0], itemKeys[1], itemKeys[2]));
                }

                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        #endregion

        #region UI Events

        private async Task LoadMedia((string SuperCategory, string Category, string Key) item)
        {
            Media? media = MediaCollection.FirstOrDefault(media => media.LibraryName == item.SuperCategory && media.SeriesName == item.Category && media.Url == item.Key);

            if (media == null || media.LibraryName == CurrentMedia.LibraryName && media.SeriesName == CurrentMedia.SeriesName && media.Url == CurrentMedia.Url)
                return;

            CurrentMedia = media;
            string itemKey = $"{media.LibraryName}|~|{media.SeriesName}|~|{media.Url}";
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(ItemKey), itemKey), false);

            await ProtectedLocalStorage.SetAsync("MediaList-ItemKey", itemKey);

            await UpdatePageState();

            await InvokeAsync(StateHasChanged);

        }

        private async Task DeleteMedia((string SuperCategory, string Category, string Key) item)
        {
            Media? media = MediaCollection.FirstOrDefault(media => media.LibraryName == item.SuperCategory && media.SeriesName == item.Category && media.Url == item.Key);

            if (media == null)
                return;

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Delete the media?",
                Body = new MarkupString($"<p>Really delete the media \"{media.Title}\"?</p>"),
                CancelChoice = "Cancel",
                Choice = "Delete",
                ChoiceColour = "danger",
                ChoiceAction = async () =>
                {
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

                    toothPickContext.Remove(media);
                    await toothPickContext.SaveChangesAsync();

                    await ModalPromptReference.ShowModalPrompt(new()
                    {
                        Title = "Succesfully deleted!",
                        Body = new MarkupString($"<p>Succesfully deleted the media \"{media.Title}\"!</p>"),
                        CancelChoice = "Dismiss"
                    });

                    await UpdatePageState();
                }
            });

        }

        private async Task ToggleOrder()
        {
            IsAscending = !IsAscending;
            string isAscendingQuery = IsAscending.ToString();
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(IsAscending), IsAscending.ToString()), false);

            if (!string.IsNullOrWhiteSpace(isAscendingQuery))
                await ProtectedLocalStorage.SetAsync("MediaList-IsAscending", isAscendingQuery);

            await UpdateMediaKeys();
        }

        private async Task SelectMediaOrder(MediaControlFields mediaOrder)
        {
            SelectedMediaOrder = mediaOrder;
            string mediaOrderQuery = mediaOrder.ToString();
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), mediaOrder.ToString()), false);

            if (!string.IsNullOrWhiteSpace(mediaOrderQuery))
                await ProtectedLocalStorage.SetAsync("MediaList-Order", mediaOrderQuery);

            await UpdateMediaKeys();
        }

        private async Task FilterMedia(Dictionary<string, string> filters)
        {
            Filters = filters;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Filters), string.Join(";", Filters.Select(keyValuePair => $"{keyValuePair.Key}:{keyValuePair.Value}"))), false);
            string filtersQuery = string.Join(";", Filters.Select(keyValuePair => $"{keyValuePair.Key}:{keyValuePair.Value}"));

            if (!string.IsNullOrWhiteSpace(filtersQuery))
                await ProtectedLocalStorage.SetAsync("MediaList-Filters", filtersQuery);

            await UpdateMediaKeys();
        }


        #endregion

        #region Helper Methods

        private async Task UpdatePageState()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            MediaCollection = toothPickContext.Media.ToArray();

            await UpdateMediaKeys();

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task UpdateMediaKeys()
        {
            IsUpdating = true;
            await InvokeAsync(StateHasChanged);

            ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            string globalFilter = Filters[string.Empty];

            MediaKeys = MediaCollection.AsQueryable()
                .Where(media =>
                    (
                    media.LibraryName.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    media.SeriesName.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    media.Url.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    media.Title.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    media.Description.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    (media.SeasonNumber.HasValue ? media.SeasonNumber.Value.ToString() : string.Empty).Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    (media.EpisodeNumber.HasValue ? media.EpisodeNumber.Value.ToString() : string.Empty).Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    (media.Duration.HasValue ? media.Duration.Value.ToString() : string.Empty).Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    media.ThumbnailLocation.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) ||
                    (media.DatePublished.HasValue ? media.DatePublished.Value.ToString() : string.Empty).Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase)) &&
                    media.LibraryName.Contains(Filters[MediaControlFields.LibraryName.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    media.SeriesName.Contains(Filters[MediaControlFields.SeriesName.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    media.Url.Contains(Filters[MediaControlFields.Url.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    media.Title.Contains(Filters[MediaControlFields.Title.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    media.Description.Contains(Filters[MediaControlFields.Description.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    (media.SeasonNumber.HasValue ? media.SeasonNumber.Value.ToString() : string.Empty).Contains(Filters[MediaControlFields.SeasonNumber.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    (media.EpisodeNumber.HasValue ? media.EpisodeNumber.Value.ToString() : string.Empty).Contains(Filters[MediaControlFields.EpisodeNumber.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    (media.Duration.HasValue ? media.Duration.Value.ToString() : string.Empty).Contains(Filters[MediaControlFields.Duration.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    media.ThumbnailLocation.Contains(Filters[MediaControlFields.ThumbnailLocation.ToString()], StringComparison.CurrentCultureIgnoreCase) &&
                    (media.DatePublished.HasValue ? media.DatePublished.Value.ToString() : string.Empty).Contains(Filters[MediaControlFields.Duration.ToString()], StringComparison.CurrentCultureIgnoreCase))
                .OrderBy($"{nameof(Location.LibraryName)} ASC")
                .ThenBy($"{nameof(Location.SeriesName)} ASC")
                .ThenBy($"{GetSortingField()} {(IsAscending ? "ASC" : "DESC")}")
                .ToDictionary(media => (media.LibraryName, media.SeriesName, media.Url), media => string.IsNullOrWhiteSpace(media.Title) ? media.Url : media.Title);

            IsUpdating = false;
            await InvokeAsync(StateHasChanged);
        }

        private string GetSortingField() => SelectedMediaOrder switch
        {
            MediaControlFields.DatePublished => nameof(Media.DatePublished),
            MediaControlFields.ThumbnailLocation => nameof(Media.ThumbnailLocation),
            MediaControlFields.Duration => nameof(Media.Duration),
            MediaControlFields.EpisodeNumber => nameof(Media.EpisodeNumber),
            MediaControlFields.Description => nameof(Media.Description),
            MediaControlFields.Url => nameof(Media.Url),
            MediaControlFields.SeriesName => nameof(Media.SeriesName),
            MediaControlFields.LibraryName => nameof(Media.LibraryName),
            MediaControlFields.Title or _ => nameof(Media.Title)
        };

        #endregion
    }
}