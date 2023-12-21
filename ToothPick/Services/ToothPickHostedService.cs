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

                Setting toothPickEnabledSetting = await toothPickContext.Settings.GetSettingAsync("ToothPickEnabled", stoppingToken);
                bool toothPickEnabled = !bool.TryParse(toothPickEnabledSetting.Value, out bool parsedToothPickEnabled) || parsedToothPickEnabled;

                if (toothPickEnabled &&
                    toothPickContext.Locations.Any())
                {
                    await new YoutubeDL()
                    {
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

                        int fetcherCount = await ProcessorMax("ParallelFetch",toothPickContext, processingStoppingToken);

                        BlockingCollection<YoutubeDL> fetchers = [];
                        for (int count = 0; count < fetcherCount; count++)
                            fetchers.Add(new YoutubeDL
                            {
                                YoutubeDLPath = "yt-dlp"
                            }, processingStoppingToken);

                        int downloaderCount = await ProcessorMax("ParallelDownloads",toothPickContext, processingStoppingToken);

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
                                foreach ((string libraryName, string serieName) in toothPickContext.Series.ToList().Select(series => (series.LibraryName, series.Name)))
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

            await GotifyService.PushMessage("ToothPick Information", $"ToothPick service stopped!", LogLevel.Information, stoppingToken);
        }

        #endregion

        #region Main Processing

        public async Task ProcessSeries(string libraryName, string serieName, YoutubeDL fetcher, BlockingCollection<MediaDownload> mediasToDownload, CancellationToken stoppingToken)
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);
            Series? series = toothPickContext.Series.FirstOrDefault(series => series.LibraryName.Equals(libraryName) && series.Name.Equals(serieName));

            if (series == null)
                return;

            try
            {
                CancellationTokenSource serieCancellationTokenSource = new();
                CancellationToken serieStoppingToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, serieCancellationTokenSource.Token).Token;
                if (StatusService.Statuses.TryAdd(series, new Status
                {
                    Series = series,
                    SerieCancellationTokenSource = serieCancellationTokenSource
                }))
                {
                    foreach (Func<Task> updateDelegate in StatusService.UpdateStatusesDelegates.Values)
                        await updateDelegate.Invoke();
                }

                foreach (Location location in series.Locations)
                {
                    if (serieStoppingToken.IsCancellationRequested)
                        return;

                    string userAgent = (await toothPickContext.Settings.GetSettingAsync("UserAgent", serieStoppingToken)).Value;
                    string limitRate = (await toothPickContext.Settings.GetSettingAsync("TotalRateLimitMBPS", serieStoppingToken)).Value;
                    int? limitRateMBPS = int.TryParse(limitRate, out int parsedSettingCount) ? parsedSettingCount : null;

                    OptionSet optionSet = new() { NoCacheDir = true, RemoveCacheDir = true, EmbedSubs = true, SubLangs = "all", AddHeader = $"User-Agent:{userAgent}", MatchFilters = "!is_live" };
                    if (!string.IsNullOrWhiteSpace(location.DownloadFormat))
                        optionSet.Format = location.DownloadFormat;
                    if (!string.IsNullOrWhiteSpace(location.Cookies))
                    {
                        string cookiesPath = (await toothPickContext.Settings.GetSettingAsync("CookiesPath", stoppingToken)).Value;
                        FileInfo cookiesFile = new(Path.Combine(cookiesPath, location.Cookies));
                        if (cookiesFile.Exists)
                            optionSet.Cookies = cookiesFile.FullName;
                    }
                    if (!string.IsNullOrWhiteSpace(location.MatchFilters))
                        optionSet.MatchFilters += $" & {location.MatchFilters}";
                    if (limitRateMBPS.HasValue)
                    {
                        int downloaderCount = await ProcessorMax("ParallelDownloads",toothPickContext, serieStoppingToken);
                        optionSet.LimitRate = limitRateMBPS.Value / downloaderCount * 1024 * 1024;
                    }

                    Setting newSeriesFetchCountOverride = await toothPickContext.Settings.GetSettingAsync("NewSeriesFetchCountOverride", serieStoppingToken);
                    int newSeriesFetchCount = int.TryParse(newSeriesFetchCountOverride.Value, out int parsedNewSeriesFetchCountOverride) ? parsedNewSeriesFetchCountOverride : Defaults.NewSeriesFetchCountOverride;

                    int itemsToFetch = newSeriesFetchCount;
                    itemsToFetch = (itemsToFetch > 0 && location.Series?.Medias.Count == 0) || !location.FetchCount.HasValue ? itemsToFetch : location.FetchCount.Value;
                    optionSet.PlaylistItems = $"1:{itemsToFetch}";

                    BlockingCollection<MediaDownload> mediaDownloads = [];
                    async Task videoDataCallback(VideoData videoData)
                    {
                        if (series.Medias.Any(item => item.Key.Equals(videoData.ID)))
                            return;

                        Media newMedia = new()
                        {
                            LibraryName = location.LibraryName,
                            SeriesName = location.SeriesName,
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
                                newMedia.EpisodeNumber = (series.Medias.Where(media => media.SeasonNumber == newMedia.SeasonNumber)?.Max(media => media.EpisodeNumber) ?? 0) + 1;
                        }

                        MediaDownload mediaDownload = new()
                        {
                            Media = newMedia,
                            Location = location,
                            OptionSet = optionSet
                        };

                        mediasToDownload.Add(mediaDownload, serieStoppingToken);
                        series.Medias.Add(newMedia);

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

                    if (!runResult.Success)
                    {
                        await RegisterErrorDownload(new()
                        {
                            LibraryName = location.LibraryName,
                            Library = location.Library,
                            SeriesName = location.SeriesName,
                            Series = location.Series,
                            Location = location.Url
                        }, string.Join(System.Environment.NewLine, runResult.ErrorOutput));
                        await GotifyService.PushMessage("ToothPick Error", $"Error fetching data for {location.LibraryName} series {location.SeriesName} location: {location.Url}; {string.Join(System.Environment.NewLine, runResult.ErrorOutput)}", LogLevel.Error, serieStoppingToken);
                        Logger.LogInformation("Error ({dateTime}) - Error fetching data for {libraryName} series {serieName} location: {location}; {errors}", DateTime.Now, location.LibraryName, location.SeriesName, location.Url, string.Join(System.Environment.NewLine, runResult.ErrorOutput));
                    }
                }

                await SaveSeriesMetadata(series, toothPickContext, serieStoppingToken);
            }
            catch (TaskCanceledException)
            {
                await GotifyService.PushMessage("ToothPick Debug", $"Canceled processing of {series.LibraryName} series {series.Name}.", LogLevel.Debug, stoppingToken);
                Logger.LogInformation("Debug ({dateTime}) - Canceled processing of {libraryName} series {serieName}.", DateTime.Now, series.LibraryName, series.Name);
            }
            catch (Exception exception)
            {
                await GotifyService.PushMessage("ToothPick Error", $"Error processing {libraryName} series: {serieName}: {exception.Message}{System.Environment.NewLine}{exception.StackTrace}", LogLevel.Error, stoppingToken);
                Logger.LogError("Error ({dateTime}) - Error processing {libraryName} series: {serieName}: {message}", DateTime.Now, libraryName, serieName, exception.Message + System.Environment.NewLine + exception.StackTrace);
            }
            finally
            {
                if (StatusService.Statuses.TryRemove(series, out _))                
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

            await GotifyService.PushMessage("ToothPick Debug", $"Downloading {media.LibraryName} series {media.SeriesName} media: {media.Title}", LogLevel.Debug, stoppingToken);
            Logger.LogInformation("Debug ({dateTime}) - Downloading {libraryName} series {serieName} media: {mediaTitle}.", DateTime.Now, media.LibraryName, media.SeriesName, media.Title);

            Progress<DownloadProgress> downloadProgress = new(async downloadProgress =>
            {
                if (DownloadsService.Downloads.TryGetValue(media, out Download? download) && download != null)
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
                await GotifyService.PushMessage("ToothPick Debug", $"Canceled download of {media.LibraryName} series {media.SeriesName} media: {media.Title}", LogLevel.Debug, stoppingToken);
                Logger.LogInformation("Debug ({dateTime}) - Canceled download of {libraryName} series {serieName} media: {mediaTitle}.", DateTime.Now, media.LibraryName, media.SeriesName, media.Title);
            }
            finally
            {
                await RemoveMediaFromDownloadService(media);
            }
        }

        private async Task ProcessRunResult(Media media, RunResult<string> runResult, CancellationToken stoppingToken)
        {
            if (runResult.Success)
            {
                await GotifyService.PushMessage("ToothPick Information", $"Succesfully downloaded {media.LibraryName} series {media.SeriesName} media: {media.Title}", LogLevel.Information, stoppingToken);
                Logger.LogInformation("Information ({dateTime}) - Succesfully downloaded {libraryName} series {serieName} media: {mediaTitle}.", DateTime.Now, media.LibraryName, media.SeriesName, media.Title);

                try
                {
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);

                    string downloadPath = (await toothPickContext.Settings.GetSettingAsync("DownloadPath", stoppingToken)).Value;
                    DirectoryInfo serieDirectory = Directory.CreateDirectory(Path.Combine(downloadPath, media.LibraryName, media.SeriesName));

                    string downloadedVideoLocation = Path.GetFullPath(runResult.Data.Trim('\'').Replace("\'\"\'\"\'", "\'"));
                    FileInfo destinationFileInfo = MoveMedia(media,
                        downloadedVideoLocation,
                        Path.Combine(serieDirectory.FullName, SanitizeFileName($"{media.Title}{Path.GetExtension(downloadedVideoLocation)}")));

                    toothPickContext.Media.Add(media);
                    await toothPickContext.SaveChangesAsync(stoppingToken);

                    try
                    {
                        await DownloadFile(toothPickContext, media.ThumbnailLocation, Path.Combine(serieDirectory.FullName, $"{Path.GetFileNameWithoutExtension(destinationFileInfo.Name)}-thumb.jpg"), stoppingToken);
                    }
                    catch (WebException webException)
                    {
                        await GotifyService.PushMessage("ToothPick Debug", $"Could not download image for {media.LibraryName} series {media.SeriesName} media {media.Title} at \"{media.ThumbnailLocation}\"; {webException}", LogLevel.Debug, stoppingToken);
                        Logger.LogInformation("Debug ({dateTime}) - Could not download image for {libraryName} series {serieName} media {mediaTitle} at \"{thumbnailLocation}\"; {webException}", DateTime.Now, media.LibraryName, media.SeriesName, media.Title, media.ThumbnailLocation, webException);
                    }
                }
                catch (Exception exception)
                {
                    await RegisterErrorDownload(media, string.Join(Environment.NewLine, exception.Message));
                    await GotifyService.PushMessage("ToothPick Error", $"There was a problem moving the downloaded {media.LibraryName} series {media.SeriesName} media: {media.Title}; {string.Join(Environment.NewLine, runResult.ErrorOutput, exception)}", LogLevel.Error, stoppingToken);
                    Logger.LogError("Error ({dateTime}) - There was a problem moving the downloaded {libraryName} series {serieName} media: {mediaTitle}; {errors}", DateTime.Now, media.LibraryName, media.SeriesName, media.Title, string.Join(Environment.NewLine, runResult.ErrorOutput, exception));
                }
            }
            else
            {
                await RegisterErrorDownload(media, string.Join(Environment.NewLine, runResult.ErrorOutput));
                await GotifyService.PushMessage("ToothPick Error", $"Could not download {media.LibraryName} series {media.SeriesName} media: {media.Location}; {string.Join(Environment.NewLine, runResult.ErrorOutput)}", LogLevel.Error, stoppingToken);
                Logger.LogError("Error ({dateTime}) - Could not download {libraryName} series {serieName} media: {mediaLocation}; {errors}", DateTime.Now, media.LibraryName, media.SeriesName, media.Location, string.Join(Environment.NewLine, runResult.ErrorOutput));
            }
        }

        private static FileInfo MoveMedia(Media media, string sourceFile, string destinationFile)
        {
            FileInfo destinationFileInfo = new(destinationFile);

            destinationFile = GetEpisodeFileName(media.SeasonNumber, media.EpisodeNumber, destinationFileInfo.DirectoryName ?? string.Empty, destinationFileInfo.Name, destinationFileInfo.Extension);

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
                    destinationFile = GetEpisodeFileName(media.SeasonNumber, media.EpisodeNumber, destinationFileInfo.DirectoryName ?? string.Empty, destinationFileInfo.Name, destinationFileInfo.Extension, removeCount, "~");
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

                    if (DownloadsService.Downloads.TryGetValue(newMedia, out Download? download) && download != null)
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

        private async Task SaveSeriesMetadata(Series series, ToothPickContext toothPickContext, CancellationToken stoppingToken)
        {
            string downloadPath = (await toothPickContext.Settings.GetSettingAsync("DownloadPath", stoppingToken)).Value;
            DirectoryInfo serieDirectory = Directory.CreateDirectory(Path.Combine(downloadPath, series.LibraryName, series.Name));

            if (!string.IsNullOrWhiteSpace(series.ThumbnailLocation) && serieDirectory.GetFiles($"thumb.jpg").Length == 0)
            {
                await DownloadFile(toothPickContext, series.ThumbnailLocation, Path.Combine(serieDirectory.FullName, $"thumb.jpg"), stoppingToken);
            }
            if (!string.IsNullOrWhiteSpace(series.PosterLocation) && serieDirectory.GetFiles($"poster.jpg").Length == 0)
            {
                await DownloadFile(toothPickContext, series.PosterLocation, Path.Combine(serieDirectory.FullName, $"poster.jpg"), stoppingToken);
            }
            if (!string.IsNullOrWhiteSpace(series.BannerLocation) && serieDirectory.GetFiles($"banner.jpg").Length == 0)
            {
                await DownloadFile(toothPickContext, series.BannerLocation, Path.Combine(serieDirectory.FullName, $"banner.jpg"), stoppingToken);
            }
            if (!string.IsNullOrWhiteSpace(series.LogoLocation) && serieDirectory.GetFiles($"logo.jpg").Length == 0)
            {
                await DownloadFile(toothPickContext, series.LogoLocation, Path.Combine(serieDirectory.FullName, $"logo.jpg"), stoppingToken);
            }
        }

        private static string GetEpisodeFileName(int? seasonNumber, int? episodeNumber, string path, string fileName, string extension, int removeCount = 0, string append = "") =>
            Path.Combine(path, $"{new string(Path.GetFileNameWithoutExtension(fileName).SkipLast(removeCount).ToArray())}{append}{(seasonNumber != null ? $"S{seasonNumber:00}" : string.Empty)}{(episodeNumber != null ? $"E{episodeNumber:00}" : string.Empty)}{extension}");

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

        #region Helper Methods
        
        private static async Task<int> ProcessorMax(string settingName, ToothPickContext toothPickContext, CancellationToken processingStoppingToken)
        {
            Setting setting = await toothPickContext.Settings.GetSettingAsync(settingName, processingStoppingToken);
            int settingCount = int.TryParse(setting.Value, out int parsedSettingCount) ? parsedSettingCount : (int)(typeof(Defaults).GetProperty(settingName)?.GetConstantValue() ?? 1);
            settingCount = settingCount < 1 || settingCount > Math.Min(4, System.Environment.ProcessorCount) ? Math.Min(4, System.Environment.ProcessorCount) : settingCount;
            return settingCount;
        }

        #endregion
    }
}