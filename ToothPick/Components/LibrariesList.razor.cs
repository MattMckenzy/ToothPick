namespace ToothPick.Components
{
    public partial class LibrariesList
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


        private IEnumerable<Library> LibrariesCollection { get; set; } = Array.Empty<Library>();
        private Dictionary<string, string>? LibrariesKeys { get; set; }
        private Dictionary<string, IEnumerable<string>>? CategoryKeys { get; set; }

        private Library CurrentLibrary { get; set; } = new();

        private bool CurrentLibraryKeyLocked = false;
        private bool CurrentLibraryIsDirty = false;
        private bool CurrentLibraryIsValid
        {
            get
            {
                return string.IsNullOrWhiteSpace(value: NameFeedback);
            }
        }

        private string NameFeedback = string.Empty;

        private ModalPrompt? ModalPromptReference = null;

        private bool IsAscending { get; set; } = true;
        private LibraryControlFields SelectedLibraryOrder { get; set; } = LibraryControlFields.Name;
        private Dictionary<string, string> Filters { get; set; } = [];

        #endregion

        #region Lifecycle Overrides

        protected override Task OnInitializedAsync()
        {
            if (!Filters.ContainsKey(string.Empty))
                Filters.Add(string.Empty, string.Empty);

            foreach (LibraryControlFields libraryControlFields in Enum.GetValues<LibraryControlFields>())
            {
                if (!Filters.ContainsKey(libraryControlFields.ToString()))
                    Filters.Add(libraryControlFields.ToString(), string.Empty);
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

                SelectedLibraryOrder = Enum.TryParse(Order, out LibraryControlFields parsedOrder) ? parsedOrder : LibraryControlFields.Name;
                NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), SelectedLibraryOrder.ToString()), false);

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

                await UpdateLibrariesKeys();

                await UpdatePageState();

                if (ItemKey != null && LibrariesCollection.Any(library => library.Name.ToString().Equals(ItemKey)))
                    await LoadLibrary(ItemKey);
                else
                    await CreateLibrary();

                IsLoading = false;
                await InvokeAsync(StateHasChanged);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        #endregion

        #region UI Events

        private async Task UpdateProperty(string propertyName, object value)
        {
            typeof(Library).GetProperty(propertyName)!.SetValue(CurrentLibrary, value);

            await UpdatePageState();
        }

        private async Task CreateLibrary()
        {
            async void createLibrary()
            {
                CurrentLibraryKeyLocked = false;

                await SetCurrentLibrary(new Library());
            }

            if (CurrentLibraryIsDirty)
                await ModalPromptReference!.ShowModalPrompt(new()
                {
                    Title = "Discard changes?",
                    Body = new MarkupString($"<p>Create a new library and discard current changes?</p>"),
                    CancelChoice = "Cancel",
                    Choice = "Yes",
                    ChoiceColour = "danger",
                    ChoiceAction = createLibrary
                });
            else
                createLibrary();
        }

        private async Task LoadLibrary(string libraryKey)
        {
            if (libraryKey.Equals(CurrentLibrary.Name, StringComparison.InvariantCultureIgnoreCase))
                return;

            Library? library = LibrariesCollection.FirstOrDefault(library => library.Name.Equals(libraryKey, StringComparison.InvariantCultureIgnoreCase));

            if (library == null)
                return;

            async void loadAction()
            {
                CurrentLibraryKeyLocked = true;

                await SetCurrentLibrary(library);

                await InvokeAsync(StateHasChanged);
            }

            if (CurrentLibraryIsDirty)
                await ModalPromptReference!.ShowModalPrompt(new()
                {
                    Title = "Discard Changes?",
                    Body = new MarkupString($"<p>Load the library: \"{library.Name}\" and discard current changes?</p>"),
                    CancelChoice = "Cancel",
                    Choice = "Yes",
                    ChoiceColour = "danger",
                    ChoiceAction = loadAction
                });
            else
                loadAction();
        }

        private async Task SaveLibrary()
        {
            if (!CurrentLibraryIsValid || !CurrentLibraryIsDirty)
                return;

            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            Library? library = await toothPickContext.Libraries.FirstOrDefaultAsync(library => library.Name == CurrentLibrary.Name);

            using ToothPickContext toothPickContext2 = await ToothPickContextFactory.CreateDbContextAsync();
            MarkupString body = default;
            if (library == null)
            {
                body = new($"<p>Succesfully added the new library \"{CurrentLibrary.Name}\"!</p>");
                toothPickContext2.Libraries.Add(CurrentLibrary);
            }
            else
            {
                body = new($"<p>Succesfully updated the library \"{CurrentLibrary.Name}\"!</p>");
                toothPickContext2.Libraries.Update(CurrentLibrary);
            }

            await toothPickContext2.SaveChangesAsync();

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Saved Library",
                Body = body,
                CancelChoice = "Dismiss"
            });

            CurrentLibraryKeyLocked = true;

            await UpdatePageState();
        }

        private async Task DeleteLibrary(string libraryKey)
        {
            Library? library = LibrariesCollection.FirstOrDefault(library => library.Name.Equals(libraryKey, StringComparison.InvariantCultureIgnoreCase));

            if (library == null)
                return;

            await ModalPromptReference!.ShowModalPrompt(new()
            {
                Title = "Delete the library?",
                Body = new MarkupString($"<p>Really delete the library \"{library.Name}\"? This will also delete all child series, locations and media.</p>"),
                CancelChoice = "Cancel",
                Choice = "Delete",
                ChoiceColour = "danger",
                ChoiceAction = async () =>
                {
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

                    toothPickContext.Remove(library);
                    await toothPickContext.SaveChangesAsync();

                    await ModalPromptReference.ShowModalPrompt(new()
                    {
                        Title = "Succesfully deleted!",
                        Body = new MarkupString($"<p>Succesfully deleted the library \"{library.Name}\"!</p>"),
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
            await UpdateLibrariesKeys();
        }

        private async Task SelectLibraryOrder(LibraryControlFields libraryOrder)
        {
            SelectedLibraryOrder = libraryOrder;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Order), libraryOrder.ToString()), false);
            await UpdateLibrariesKeys();
        }

        private async Task FilterLibraries(Dictionary<string, string> filters)
        {
            Filters = filters;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(Filters), string.Join(";", Filters.Select(keyValuePair => $"{keyValuePair.Key}:{keyValuePair.Value}"))), false);
            await UpdateLibrariesKeys();
        }


        #endregion

        #region Helper Methods

        private async Task UpdatePageState()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            ValidateCurrentLibrary();

            CurrentLibraryIsDirty = (!CurrentLibraryKeyLocked && !CompareLibraries(CurrentLibrary, new Library() )) ||
                (CurrentLibraryKeyLocked && !CompareLibraries(CurrentLibrary, toothPickContext.Libraries.ToArray().FirstOrDefault(library => library.Name == CurrentLibrary.Name)));

            LibrariesCollection = toothPickContext.Libraries.ToArray();

            await UpdateLibrariesKeys();

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }

        private void ValidateCurrentLibrary()
        {
            // Validate Library.Name
            NameFeedback = string.Empty;
            if (!CurrentLibraryKeyLocked)
            {
                if (string.IsNullOrWhiteSpace(CurrentLibrary.Name))
                    NameFeedback = "A library name is required!";
                else if (LibrariesCollection.FirstOrDefault(library => library.Name == CurrentLibrary.Name) != null)
                    NameFeedback = "The library name must be unique! ";
            }
        }

        private static bool CompareLibraries(Library? library1, Library? library2)
        {
            if (library1 == null || library2 == null)
                return false;

            return 
                library1.Name.Equals(library2.Name, StringComparison.InvariantCulture);
        }

        private async Task SetCurrentLibrary(Library library)
        {
            CurrentLibrary = library;
            NavigationManager.NavigateTo(NavigationManager.GetUriWithQueryParameter(nameof(ItemKey), library.Name), false);
            await UpdatePageState();
        }

        private async Task UpdateLibrariesKeys()
        {
            IsUpdating = true;
            await InvokeAsync(StateHasChanged);

            ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            string globalFilter = Filters[string.Empty];

            LibrariesKeys = LibrariesCollection.AsQueryable()
                .Where(library =>
                    library.Name.Contains(globalFilter, StringComparison.CurrentCultureIgnoreCase) &&
                    library.Name.Contains(Filters[LibraryControlFields.Name.ToString()], StringComparison.CurrentCultureIgnoreCase))
                .OrderBy($"{GetSortingField()} {(IsAscending ? "ASC" : "DESC")}")
                .ToDictionary(library => library.Name, library => library.Name);
           
            IsUpdating = false;
            await InvokeAsync(StateHasChanged);
        }

        private string GetSortingField() => SelectedLibraryOrder switch
        {
            LibraryControlFields.Name or _ => nameof(Library.Name)
        };

        #endregion
    }
}