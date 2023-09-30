using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace ToothPick.Services
{
    public partial class ToothPickHostedService : BackgroundService
    {
        #region Private Properties

        private IDbContextFactory<ToothPickContext> ToothPickContextFactory { get; set; } = null!;
        private readonly GotifyService GotifyService;
        private readonly DownloadsService DownloadsService;
        private readonly StatusService StatusService;
        private readonly ILogger<ToothPickHostedService> Logger;

        private static readonly char[] InvalidFilenameCharacters = ['|', '*', '/', ':', ';', '?', '\\', '"', '<', '>'];
        private static readonly string[] ValidImageExtensions = [".jpg", ".jpeg", ".gif", ".png"];

        #endregion

        #region Constructor and Entry Point

        public ToothPickHostedService(IDbContextFactory<ToothPickContext> toothPickContextFactory, GotifyService gotifyService, StatusService statusService, DownloadsService downloadsService, ILogger<ToothPickHostedService> logger)
        {
            ToothPickContextFactory = toothPickContextFactory;
            GotifyService = gotifyService;
            DownloadsService = downloadsService;
            StatusService = statusService;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await GotifyService.PushMessage("ToothPick Information", $"ToothPick service started!", LogLevel.Information, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);
                
                Setting toothPickEnabledSetting = await toothPickContext.Settings.GetSettingAsync("ToothPickEnabled", stoppingToken );
                bool toothPickEnabled = !bool.TryParse(toothPickEnabledSetting.Value, out bool parsedToothPickEnabled) || parsedToothPickEnabled;

                if (toothPickEnabled &&
                    toothPickContext.Locations.Any())
                {
                    await new YoutubeDL() {
                        YoutubeDLPath = "yt-dlp"
                    }.RunUpdate("nightly");
                
                    try
                    {
                        StatusService.ProcessingPercent = 0;
                        StatusService.TotalProcessingSeries = toothPickContext.Series.Count();
                        StatusService.ProcessingCancellationTokenSource = new CancellationTokenSource();
                        StatusService.NextProcessingTime = DateTime.Now.ToLocalTime();
                        foreach (Func<Task> updateDelegate in StatusService.UpdateProcessingDelegates.Values)
                            await updateDelegate.Invoke();

                        CancellationToken processingStoppingToken =
                        CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, StatusService.ProcessingCancellationTokenSource.Token).Token;

                        Setting fetcherCountSetting = await toothPickContext.Settings.GetSettingAsync("ParallelFetch", processingStoppingToken);
                        int fetcherCount = int.TryParse(fetcherCountSetting.Value, out int parsedFetcherCount) ? parsedFetcherCount : Defaults.ParallelFetch;
                        fetcherCount = fetcherCount < 1 || fetcherCount > Math.Min(4, System.Environment.ProcessorCount) ? Math.Min(4, System.Environment.ProcessorCount) : fetcherCount;

                        BlockingCollection<YoutubeDL> fetchers = [];
                        for (int count = 0; count < fetcherCount; count++)
                            fetchers.Add(new YoutubeDL
                            {
                                YoutubeDLPath = "yt-dlp"
                            }, processingStoppingToken);

                        Setting downloaderCountSetting = await toothPickContext.Settings.GetSettingAsync("ParallelDownloads", processingStoppingToken);
                        int downloaderCount = int.TryParse(downloaderCountSetting.Value, out int parsedDownloaderCount) ? parsedDownloaderCount : Defaults.ParallelDownloads;
                        downloaderCount = downloaderCount < 1 || downloaderCount > Math.Min(4, System.Environment.ProcessorCount) ? Math.Min(4, System.Environment.ProcessorCount) : downloaderCount;

                        string downloadPath = (await toothPickContext.Settings.GetSettingAsync("DownloadPath", processingStoppingToken)).Value;

                        BlockingCollection<YoutubeDL> downloaders = [];
                        for (int count = 0; count < downloaderCount; count++)
                            downloaders.Add(new YoutubeDL
                            {
                                YoutubeDLPath = "yt-dlp",
                                FFmpegPath = "ffmpeg",
                                OutputFolder = downloadPath,
                                RestrictFilenames = true
                            }, processingStoppingToken);

                        BlockingCollection<MediaDownload> mediasToDownload = [];

                        List<Task> tasks =
                        [
                            Task.Run(async () => {
                                List<Task> fetchingTasks = [];
                                foreach ((string libraryName, string serieName) in toothPickContext.Series.ToList().Select(serie => (serie.LibraryName, serie.Name)))
                                {
                                    YoutubeDL fetcher = fetchers.Take(processingStoppingToken);

                                    if (processingStoppingToken.IsCancellationRequested)
                                    return;

                                    fetchingTasks.Add(Task.Run(async () =>
                                    {
                                        await ProcessSeries(libraryName, serieName, fetcher, mediasToDownload, processingStoppingToken);
                                        fetchers.Add(fetcher);
                                    }));
                                }
                                await Task.WhenAll(fetchingTasks);
                                mediasToDownload.CompleteAdding();
                            }, processingStoppingToken),
                            
                            Task.Run(async () => {
                                List<Task> downloadingTasks = [];

                                try
                                {
                                    while (!mediasToDownload.IsCompleted)
                                    {
                                        MediaDownload mediaDownload = mediasToDownload.Take(processingStoppingToken);
                                        YoutubeDL downloader = downloaders.Take(processingStoppingToken);

                                        if (processingStoppingToken.IsCancellationRequested)
                                            return;

                                        downloadingTasks.Add(Task.Run(async () =>
                                        {
                                            await DownloadMedia(mediaDownload, downloader, processingStoppingToken);
                                            downloaders.Add(downloader);
                                        }));
                                    }
                                }
                                catch (TaskCanceledException)
                                {
                                    await GotifyService.PushMessage("ToothPick Debug", $"Canceled processing.", LogLevel.Debug, stoppingToken);
                                    Logger.LogInformation("Debug ({dateTime}) - Canceled processing.", DateTime.Now);
                                }
                                catch (InvalidOperationException)
                                {
                                    await GotifyService.PushMessage("ToothPick Debug", $"Finished processing series.", LogLevel.Debug, stoppingToken);
                                    Logger.LogInformation("Debug ({dateTime}) - Canceled processing.", DateTime.Now);
                                }

                                await Task.WhenAll(downloadingTasks);

                            }, processingStoppingToken)
                        ];
                        List<Task> executionTasks = tasks;

                        await Task.WhenAll(executionTasks);
                    }
                    catch (TaskCanceledException)
                    {
                        await GotifyService.PushMessage("ToothPick Debug", $"Canceled processing.", LogLevel.Debug, stoppingToken);
                        Logger.LogInformation("Debug ({dateTime}) - Canceled processing.", DateTime.Now);
                    }
                    catch (Exception exception)
                    {
                        Logger.LogCritical("Critical ({dateTime}) - Exception during crawl and save: {message}", DateTime.Now, exception.Message + System.Environment.NewLine + exception.StackTrace);
                        await GotifyService.PushMessage("ToothPick Critical", $"Exception during crawl and save: {exception.Message}{System.Environment.NewLine}{exception.StackTrace}", LogLevel.Critical, stoppingToken);
                    }

                    try
                    { 
                        Setting scanDelayMinutesSetting = await toothPickContext.Settings.GetSettingAsync("ScanDelayMinutes", stoppingToken);
                        int scanDelayMinutes = int.TryParse(scanDelayMinutesSetting.Value, out int parsedScanDelayMinutes) ? parsedScanDelayMinutes : Defaults.ScanDelayMinutes;
                        
                        StatusService.ProcessingPercent = -1;
                        StatusService.TotalProcessingSeries = 0;
                        StatusService.ProcessingCancellationTokenSource = new CancellationTokenSource();
                        StatusService.NextProcessingTime = (DateTime.Now + TimeSpan.FromMinutes(scanDelayMinutes)).ToLocalTime();
                        foreach (Func<Task> updateDelegate in StatusService.UpdateProcessingDelegates.Values)
                            await updateDelegate.Invoke();

                        await Task.Delay(TimeSpan.FromMinutes(scanDelayMinutes),
                            CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, StatusService.ProcessingCancellationTokenSource.Token).Token);

                        await toothPickContext.DisposeAsync();        
                    }                              
                    catch (TaskCanceledException)
                    {
                        await GotifyService.PushMessage("ToothPick Debug", $"Canceled processing delay.", LogLevel.Debug, stoppingToken);
                        Logger.LogInformation("Debug ({dateTime}) - Canceled processing delay.", DateTime.Now);
                    }
                    catch (Exception exception)
                    {
                        Logger.LogCritical("Critical ({dateTime}) - Exception during processing delay: {message}", DateTime.Now, exception.Message + System.Environment.NewLine + exception.StackTrace);
                        await GotifyService.PushMessage("ToothPick Critical", $"Exception processing delay: {exception.Message}{System.Environment.NewLine}{exception.StackTrace}", LogLevel.Critical, stoppingToken);
                    } 
                }
            }

            await GotifyService.PushMessage("ToothPick Information", $"ToothPick service stopped!", LogLevel.Information, stoppingToken);
        }

        #endregion

        #region Service Set-Up

        private static bool GetDifferent<T>(T checkValue, ref T outValue)
        {
            if (!outValue.Equals(checkValue))
            {
                outValue = checkValue;
                return true;
            }
            else
                return false;
        }

        #endregion

        #region Main Processing

        public async Task ProcessSeries(string libraryName, string serieName, YoutubeDL fetcher, BlockingCollection<MediaDownload> mediasToDownload, CancellationToken stoppingToken)
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);
            Serie serie = toothPickContext.Series.FirstOrDefault(serie => serie.LibraryName.Equals(libraryName) && serie.Name.Equals(serieName));

            try 
            {
                CancellationTokenSource serieCancellationTokenSource = new();
                CancellationToken serieStoppingToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, serieCancellationTokenSource.Token).Token;
                if (StatusService.Statuses.TryAdd(serie, new Status
                {
                    Serie = serie,
                    SerieCancellationTokenSource = serieCancellationTokenSource
                }))
                {
                    foreach (Func<Task> updateDelegate in StatusService.UpdateStatusesDelegates.Values)
                        await updateDelegate.Invoke();
                }

                foreach (Location location in serie.Locations)
                {
                    if (serieStoppingToken.IsCancellationRequested)
                        return;

                    string userAgent = (await toothPickContext.Settings.GetSettingAsync("UserAgent", serieStoppingToken)).Value;

                    OptionSet optionSet = new() { NoCacheDir = true, RemoveCacheDir = true, EmbedSubs = true, SubLangs = "all", AddHeader = $"User-Agent:{userAgent}", MatchFilters = "live_status!='is_live' & live_status!='is_upcoming'" };
                    if (!string.IsNullOrWhiteSpace(location.DownloadFormat))
                        optionSet.Format = location.DownloadFormat;                    
                    if (!string.IsNullOrWhiteSpace(location.Cookies))
                        optionSet.Cookies = location.Cookies;
                    if (!string.IsNullOrWhiteSpace(location.MatchFilters))
                        optionSet.MatchFilters += $" & {location.MatchFilters}";

                    Setting newSeriesFetchCountOverride = await toothPickContext.Settings.GetSettingAsync("NewSeriesFetchCountOverride", serieStoppingToken);
                    int newSeriesFetchCount = int.TryParse(newSeriesFetchCountOverride.Value, out int parsedNewSeriesFetchCountOverride) ? parsedNewSeriesFetchCountOverride : Defaults.NewSeriesFetchCountOverride;
                    
                    int itemsToFetch = newSeriesFetchCount;
                    itemsToFetch = itemsToFetch > 0 && location.Serie.Medias.Count == 0 ? itemsToFetch : location.FetchCount;
                    optionSet.PlaylistItems = $"1:{itemsToFetch}";
 
                    // TODO: Add playlist filtering.

                    BlockingCollection<MediaDownload> mediaDownloads = [];
                    async Task videoDataCallback(VideoData videoData)
                    {
                        if (serie.Medias.Any(item => item.Key.Equals(videoData.ID)))
                            return;

                        Media newMedia = new()
                        {
                            LibraryName = location.LibraryName,
                            SerieName = location.SerieName,
                            Location = videoData.WebpageUrl,
                            Key = videoData.ID,
                            Title = videoData.Title,
                            Description = videoData.Description,
                            Duration = videoData.Duration,
                            ThumbnailLocation = videoData.Thumbnail,
                            DatePublished = videoData.UploadDate,
                            SeasonNumber = videoData.SeasonNumber,
                            EpisodeNumber = videoData.EpisodeNumber
                        };

                        if (!newMedia.SeasonNumber.HasValue)
                        {
                            (int? newSeasonNumber, _) = GetSeasonNumber(newMedia.Title);
                            if (newSeasonNumber.HasValue)
                                newMedia.SeasonNumber = newSeasonNumber.Value;
                            else
                                newMedia.SeasonNumber = 1;
                        }

                        if (!newMedia.EpisodeNumber.HasValue)
                        {
                            (int? newEpisodeNumber, _) = GetEpisodeNumber(newMedia.Title);
                            if (newEpisodeNumber.HasValue)
                                newMedia.EpisodeNumber = newEpisodeNumber;
                            else
                                newMedia.EpisodeNumber = (serie.Medias.Where(media => (int)media.SeasonNumber == (int)newMedia.SeasonNumber)?.Max(media => media.EpisodeNumber) ?? 0) + 1;
                        }

                        MediaDownload mediaDownload = new()
                        {
                            Media = newMedia,
                            OptionSet = optionSet
                        };

                        mediasToDownload.Add(mediaDownload, serieStoppingToken);
                        serie.Medias.Add(newMedia);

                        if (DownloadsService.Downloads.TryAdd(newMedia, new Download
                        {
                            Media = newMedia,
                            DownloadCancellationTokenSource = mediaDownload.DownloadCancellationTokenSource
                        }))
                        {
                            foreach (Func<Task> updateDelegate in DownloadsService.UpdateDelegates.Values)
                                await updateDelegate.Invoke();
                        }
                    }

                    RunResult<IEnumerable<VideoData>> runResult = await fetcher.RunPlaylistDataFetch(location.Url, videoDataCallback, ct: stoppingToken, overrideOptions: optionSet);

                    if(!runResult.Success)
                    {
                        await RegisterErrorDownload(new() { Library = location.Library, Serie = location.Serie, Location = location.Url }, string.Join(System.Environment.NewLine, runResult.ErrorOutput));
                        await GotifyService.PushMessage("ToothPick Error", $"Error fetching data for {location.LibraryName} serie {location.SerieName} location: {location.Url}; {string.Join(System.Environment.NewLine, runResult.ErrorOutput)}", LogLevel.Error, serieStoppingToken);
                        Logger.LogInformation("Error ({dateTime}) - Error fetching data for {libraryName} serie {serieName} location: {location}; {errors}", DateTime.Now, location.LibraryName, location.SerieName, location.Url, string.Join(System.Environment.NewLine, runResult.ErrorOutput));
                    }
                }

                await SaveSeriesMetadata(serie, toothPickContext, serieStoppingToken);
            }
            catch (TaskCanceledException)
            {
                await GotifyService.PushMessage("ToothPick Debug", $"Canceled processing of {serie.LibraryName} serie {serie.Name}.", LogLevel.Debug, stoppingToken);
                Logger.LogInformation("Debug ({dateTime}) - Canceled processing of {libraryName} serie {serieName}.", DateTime.Now, serie.LibraryName, serie.Name);
            }
            catch (Exception exception)
            {
                await GotifyService.PushMessage("ToothPick Error", $"Error processing {libraryName} series: {serieName}: {exception.Message}{System.Environment.NewLine}{exception.StackTrace}", LogLevel.Error, stoppingToken);
                Logger.LogError("Error ({dateTime}) - Error processing {libraryName} series: {serieName}: {message}", DateTime.Now, libraryName, serieName, exception.Message + System.Environment.NewLine + exception.StackTrace);
            }
            finally
            {
                if (StatusService.Statuses.TryRemove(serie, out _))
                {
                    foreach (Func<Task> updateDelegate in StatusService.UpdateStatusesDelegates.Values)
                        await updateDelegate.Invoke();
                };

                await toothPickContext.DisposeAsync();
                StatusService.IncrementProcessingPercent();
                foreach (Func<Task> updateDelegate in StatusService.UpdateProcessingDelegates.Values)
                    await updateDelegate.Invoke();
            }
        }

        #endregion

        #region Video Processing

        private async Task DownloadMedia(MediaDownload mediaDownload, YoutubeDL downloader, CancellationToken stoppingToken)
        {            
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);

            Media media = mediaDownload.Media;
            string location = media.Location;

            await GotifyService.PushMessage("ToothPick Debug", $"Downloading {media.LibraryName} serie {media.SerieName} media: {media.Title}", LogLevel.Debug, stoppingToken);
            Logger.LogInformation("Debug ({dateTime}) - Downloading {libraryName} serie {serieName} media: {mediaTitle}.", DateTime.Now, media.LibraryName, media.SerieName, media.Title);

            Progress<DownloadProgress> downloadProgress = new(async downloadProgress =>
            {
                if (DownloadsService.Downloads.TryGetValue(media, out Download download))
                {
                    foreach (Func<DownloadProgress, Task> updateDelegate in download.UpdateDelegates.Values)
                        await updateDelegate.Invoke(downloadProgress);
                }
            });

            try
            {
                RunResult<string> runResult = await downloader.RunVideoDownload(
                    location,
                    format: mediaDownload.OptionSet.Format,
                    progress: downloadProgress,
                    ct: CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, mediaDownload.DownloadCancellationTokenSource.Token).Token,
                    overrideOptions: mediaDownload.OptionSet);
                            
                await ProcessRunResult(media, runResult, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                await GotifyService.PushMessage("ToothPick Debug", $"Canceled download of {media.LibraryName} serie {media.SerieName} media: {media.Title}", LogLevel.Debug, stoppingToken);
                Logger.LogInformation("Debug ({dateTime}) - Canceled download of {libraryName} serie {serieName} media: {mediaTitle}.", DateTime.Now, media.LibraryName, media.SerieName, media.Title);
            }
            finally
            {
                await RemoveMediaFromDownloadService(media);
            }
        }

        private async Task ProcessRunResult(Media media, RunResult<string> runResult, CancellationToken stoppingToken)
        {
            if (runResult != null && runResult.Success)
            {
                await GotifyService.PushMessage("ToothPick Information", $"Succesfully downloaded {media.LibraryName} serie {media.SerieName} media: {media.Title}", LogLevel.Information, stoppingToken);
                Logger.LogInformation("Information ({dateTime}) - Succesfully downloaded {libraryName} serie {serieName} media: {mediaTitle}.", DateTime.Now, media.LibraryName, media.SerieName, media.Title);

                try
                {                      
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);             
                    Serie serie = toothPickContext.Series.FirstOrDefault(serie => serie.LibraryName.Equals(media.LibraryName) && serie.Name.Equals(media.SerieName));

                    string downloadPath = (await toothPickContext.Settings.GetSettingAsync("DownloadPath", stoppingToken)).Value;
                    DirectoryInfo serieDirectory = Directory.CreateDirectory(Path.Combine(downloadPath, media.LibraryName, media.SerieName));

                    string downloadedVideoLocation = Path.GetFullPath(runResult.Data.Trim('\'').Replace("\'\"\'\"\'", "\'"));
                    FileInfo destinationFileInfo = MoveMedia(media,
                        downloadedVideoLocation,
                        Path.Combine(serieDirectory.FullName, SanitizeFileName($"{media.Title}{Path.GetExtension(downloadedVideoLocation)}")));

                    toothPickContext.Medias.Add(media);
                    await toothPickContext.SaveChangesAsync(stoppingToken);

                    try
                    {
                       await DownloadFile(toothPickContext, media.ThumbnailLocation, Path.Combine(serieDirectory.FullName, $"{Path.GetFileNameWithoutExtension(destinationFileInfo.Name)}-thumb.jpg"), stoppingToken);
                    }
                    catch (WebException webException)
                    {
                       await GotifyService.PushMessage("ToothPick Debug", $"Could not download image for {media.LibraryName} serie {media.SerieName} media {media.Title} at \"{media.ThumbnailLocation}\"; {webException}", LogLevel.Debug, stoppingToken);
                       Logger.LogInformation("Debug ({dateTime}) - Could not download image for {libraryName} serie {serieName} media {mediaTitle} at \"{thumbnailLocation}\"; {webException}", DateTime.Now, media.LibraryName, media.SerieName, media.Title, media.ThumbnailLocation, webException);
                    }
                }
                catch (Exception exception)
                {
                    await RegisterErrorDownload(media, string.Join(System.Environment.NewLine, exception.Message));
                    await GotifyService.PushMessage("ToothPick Error", $"There was a problem moving the downloaded {media.LibraryName} serie {media.SerieName} media: {media.Title}; {string.Join(System.Environment.NewLine, runResult.ErrorOutput, exception)}", LogLevel.Error, stoppingToken);
                    Logger.LogError("Error ({dateTime}) - There was a problem moving the downloaded {libraryName} serie {serieName} media: {mediaTitle}; {errors}", DateTime.Now, media.LibraryName, media.SerieName, media.Title, string.Join(System.Environment.NewLine, runResult.ErrorOutput, exception));
                }
            }
            else
            {
                await RegisterErrorDownload(media, string.Join(System.Environment.NewLine, runResult.ErrorOutput));
                await GotifyService.PushMessage("ToothPick Error", $"Could not download {media.LibraryName} serie {media.SerieName} media: {media.Location}; {string.Join(System.Environment.NewLine, runResult.ErrorOutput)}", LogLevel.Error, stoppingToken);
                Logger.LogError("Error ({dateTime}) - Could not download {libraryName} serie {serieName} media: {mediaLocation}; {errors}", DateTime.Now, media.LibraryName, media.SerieName, media.Location, string.Join(System.Environment.NewLine, runResult.ErrorOutput));
            }
        }

        private static FileInfo MoveMedia(Media media, string sourceFile, string destinationFile)
        {
            FileInfo destinationFileInfo = new(destinationFile);

            destinationFile = GetEpisodeFileName(media.SeasonNumber.Value, media.EpisodeNumber.Value, destinationFileInfo.DirectoryName, destinationFileInfo.Name, destinationFileInfo.Extension);

            bool success = false;
            int removeCount = 1;
            while (!success)
            {
                try
                {
                    File.Move(sourceFile, destinationFile, true);
                    success = true;
                }
                catch (PathTooLongException)
                {
                    removeCount++;
                    destinationFile = GetEpisodeFileName(media.SeasonNumber.Value, media.EpisodeNumber.Value, destinationFileInfo.DirectoryName, destinationFileInfo.Name, destinationFileInfo.Extension, removeCount, "~");
                }
            }

            return new(destinationFile);
        }

        private async Task RemoveMediaFromDownloadService(Media video)
        {
            if (DownloadsService.Downloads.TryRemove(video, out _))
            {
                foreach (Func<Task> updateDelegate in DownloadsService.UpdateDelegates.Values)
                    await updateDelegate.Invoke();
            };
        }

        private async Task RegisterErrorDownload(Media newMedia, string errorMessage)
        {
            CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromMinutes(10));

            DownloadProgress downloadProgress = new(DownloadState.Error, data: errorMessage);

            if (DownloadsService.Downloads.TryAdd(newMedia, new Download
            {
                Media = newMedia,
                DownloadCancellationTokenSource = cancellationTokenSource
            }))
            {
                foreach (Func<Task> updateDelegate in DownloadsService.UpdateDelegates.Values)
                    await updateDelegate.Invoke();
            }

            _ = Task.Run(async () =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(3000);

                    if (DownloadsService.Downloads.TryGetValue(newMedia, out Download download))
                    {
                        foreach (Func<DownloadProgress, Task> updateDelegate in download.UpdateDelegates.Values)
                            await updateDelegate.Invoke(downloadProgress);
                    }
                }

                if (DownloadsService.Downloads.TryRemove(newMedia, out _))
                {
                    foreach (Func<Task> updateDelegate in DownloadsService.UpdateDelegates.Values)
                        await updateDelegate.Invoke();
                };
            });
        }


        #endregion

        #region Metadata Processing

        private async Task SaveSeriesMetadata(Serie serie, ToothPickContext toothPickContext, CancellationToken stoppingToken)
        {
            string downloadPath = (await toothPickContext.Settings.GetSettingAsync("DownloadPath", stoppingToken)).Value;     
            DirectoryInfo serieDirectory = Directory.CreateDirectory(Path.Combine(downloadPath, serie.LibraryName, serie.Name));

            if (!string.IsNullOrWhiteSpace(serie.ThumbnailLocation) && serieDirectory.GetFiles($"thumb.jpg").Length == 0)
            {
                await DownloadFile(toothPickContext, serie.ThumbnailLocation, Path.Combine(serieDirectory.FullName, $"thumb.jpg"), stoppingToken);
            }
            if (!string.IsNullOrWhiteSpace(serie.PosterLocation) && serieDirectory.GetFiles($"poster.jpg").Length == 0)
            {
                await DownloadFile(toothPickContext, serie.PosterLocation, Path.Combine(serieDirectory.FullName, $"poster.jpg"), stoppingToken);
            }
            if (!string.IsNullOrWhiteSpace(serie.BannerLocation) && serieDirectory.GetFiles($"banner.jpg").Length == 0)
            {
                await DownloadFile(toothPickContext, serie.BannerLocation, Path.Combine(serieDirectory.FullName, $"banner.jpg"), stoppingToken);
            }
            if (!string.IsNullOrWhiteSpace(serie.LogoLocation) && serieDirectory.GetFiles($"logo.jpg").Length == 0)
            {
                await DownloadFile(toothPickContext, serie.LogoLocation, Path.Combine(serieDirectory.FullName, $"logo.jpg"), stoppingToken);
            }
        }

        private static string GetEpisodeFileName(int seasonNumber, int episodeNumber, string path, string fileName, string extension, int removeCount = 0, string append = "") =>
            Path.Combine(path, $"{new string(Path.GetFileNameWithoutExtension(fileName).SkipLast(removeCount).ToArray())}{append} S{seasonNumber:00}E{episodeNumber:00}{extension}");

        private static (int?, Match) GetSeasonNumber(string title)
        {
            Match match = SeasonRegex().Match(title);
            if (match.Success && int.TryParse(match.Groups.Values.Last().Value, out int seasonNumber))
                return (seasonNumber, match);
            else
                return (null, match);
        }

        private static (int?, Match) GetEpisodeNumber(string title)
        {
            Match match = EpisodeRegex().Match(title);
            if (match.Success && int.TryParse(match.Groups.Values.Last().Value, out int episodeNumber))
                return (episodeNumber, match);
            else
                return (null, match);
        }

        private async Task DownloadFile(ToothPickContext toothPickContext, string fileUri, string downloadLocation, CancellationToken stoppingToken)
        {
            string userAgent = (await toothPickContext.Settings.GetSettingAsync("UserAgent", stoppingToken)).Value;

            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);

            HttpResponseMessage response = await httpClient.GetAsync(fileUri, stoppingToken);
            if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(downloadLocation) && ValidImageExtensions.Contains(Path.GetExtension(downloadLocation)))
                await File.WriteAllBytesAsync(downloadLocation, await response.Content.ReadAsByteArrayAsync(stoppingToken), stoppingToken);
        }

        private static string SanitizeFileName(string fileName)
        {
            IEnumerable<char> invalidChars = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).Union(InvalidFilenameCharacters);
            return new string(fileName.Where(currentChar => !invalidChars.Any(invalidChar => invalidChar.Equals(currentChar))).ToArray());
        }

        [GeneratedRegex("(season |s)([0-9]+)", RegexOptions.IgnoreCase, "en-CA")]
        private static partial Regex SeasonRegex();
        [GeneratedRegex("(episode |e|ep\\.|ep\\. )([0-9]+)", RegexOptions.IgnoreCase, "en-CA")]
        private static partial Regex EpisodeRegex();

    #endregion
}
}