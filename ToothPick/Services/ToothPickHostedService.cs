using System.Data;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace ToothPick.Services
{
    public class ToothPickHostedService(IDbContextFactory<ToothPickContext> toothPickContextFactory, GotifyService gotifyService, StatusService statusService, DownloadsService downloadsService, ILogger<ToothPickHostedService> logger) : BackgroundService
    {
        #region Private Properties

        private IDbContextFactory<ToothPickContext> ToothPickContextFactory { get; set; } = toothPickContextFactory;
        private readonly GotifyService GotifyService = gotifyService;
        private readonly DownloadsService DownloadsService = downloadsService;
        private readonly StatusService StatusService = statusService;
        private readonly ILogger<ToothPickHostedService> Logger = logger;

        private static readonly char[] InvalidFilenameCharacters = ['|', '*', '/', ':', ';', '?', '\\', '"', '<', '>'];
        private static readonly string[] ValidImageExtensions = [".jpg", ".jpeg", ".gif", ".png"];

        private static YoutubeDL PlaylistDataFetcher { get; } = new()
        {
            YoutubeDLPath = "yt-dlp"
        };

        private static YoutubeDL MediaDataFetcher { get; } = new()
        {
            YoutubeDLPath = "yt-dlp"
        };

        private static YoutubeDL MediaDownloader { get; } = new()
        {
            YoutubeDLPath = "yt-dlp",
            FFmpegPath = "ffmpeg",
            RestrictFilenames = true
        };

        #endregion

        #region Constructor and Entry Point

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await GotifyService.PushMessage("ToothPick Information", $"ToothPick service started!", LogLevel.Information, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);

                    Setting toothPickEnabledSetting = await toothPickContext.Settings.GetSettingAsync("ToothPickEnabled", stoppingToken);
                    bool toothPickEnabled = !bool.TryParse(toothPickEnabledSetting.Value, out bool parsedToothPickEnabled) || parsedToothPickEnabled;

                    if (toothPickEnabled &&
                        toothPickContext.Locations.Any())
                    {
                        await PlaylistDataFetcher.RunUpdate("nightly");

                        StatusService.ProcessingPercent = 0;
                        StatusService.TotalProcessingSeries = toothPickContext.Series.Count();
                        StatusService.ProcessingCancellationTokenSource = new CancellationTokenSource();
                        StatusService.NextProcessingTime = DateTime.Now.ToLocalTime();
                        foreach (Func<Task> updateDelegate in StatusService.UpdateProcessingDelegates.Values)
                            await updateDelegate.Invoke();

                        CancellationToken processingStoppingToken =
                            CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, StatusService.ProcessingCancellationTokenSource.Token).Token;

                        int parallelPlaylistFetchCount = await ProcessorMax("ParallelPlaylistFetch", toothPickContext, processingStoppingToken);
                        await PlaylistDataFetcher.SetMaxNumberOfProcesses((byte)parallelPlaylistFetchCount);

                        int parallelMediaDataFetchCount = await ProcessorMax("ParallelMediaDataFetch", toothPickContext, processingStoppingToken);
                        await MediaDataFetcher.SetMaxNumberOfProcesses((byte)parallelMediaDataFetchCount);

                        int downloaderCount = await ProcessorMax("ParallelDownloads", toothPickContext, processingStoppingToken);
                        await MediaDownloader.SetMaxNumberOfProcesses((byte)downloaderCount);

                        string downloadPath = (await toothPickContext.Settings.GetSettingAsync("DownloadPath", stoppingToken)).Value;
                        MediaDownloader.OutputFolder = downloadPath;

                        List<Task> seriesProcessingTasks = [];
                        foreach ((string LibraryName, string SeriesName) in toothPickContext.Series.ToList()
                            .OrderBy(series => series.LibraryName)
                            .ThenBy(series => series.Name)
                            .Select(series => (series.LibraryName, series.Name))
                        )
                        {
                            seriesProcessingTasks.Add(Task.Run(async () => await ProcessSeries(LibraryName, SeriesName, processingStoppingToken), stoppingToken));
                        }

                        Task.WaitAll([.. seriesProcessingTasks], stoppingToken);
                    }
                }
                catch (Exception exception) when (exception is OperationCanceledException || exception is TaskCanceledException)
                {
                    await GotifyService.PushMessage("ToothPick Debug", $"Canceled processing.", LogLevel.Debug, stoppingToken);
                    Logger.LogInformation("Debug ({dateTime}) - Canceled processing delay.", DateTime.Now);
                }
                catch (Exception exception)
                {
                    Logger.LogCritical("Critical ({dateTime}) - Exception during processing: {message}", DateTime.Now, exception.Message + Environment.NewLine + exception.StackTrace);
                    await GotifyService.PushMessage("ToothPick Critical", $"Exception processing delay: {exception.Message}{Environment.NewLine}{exception.StackTrace}", LogLevel.Critical, stoppingToken);
                }
                finally
                {
                    GC.Collect(3);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(3);
                }

                try
                {
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);

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
                catch (Exception exception) when (exception is OperationCanceledException || exception is TaskCanceledException)
                {
                    await GotifyService.PushMessage("ToothPick Debug", $"Canceled processing delay.", LogLevel.Debug, stoppingToken);
                    Logger.LogInformation("Debug ({dateTime}) - Canceled processing delay.", DateTime.Now);
                }
                catch (Exception exception)
                {
                    Logger.LogCritical("Critical ({dateTime}) - Exception during processing delay: {message}", DateTime.Now, exception.Message + Environment.NewLine + exception.StackTrace);
                    await GotifyService.PushMessage("ToothPick Critical", $"Exception processing delay: {exception.Message}{Environment.NewLine}{exception.StackTrace}", LogLevel.Critical, stoppingToken);
                }
            }

            await GotifyService.PushMessage("ToothPick Information", $"ToothPick service stopped!", LogLevel.Information, stoppingToken);
        }

        #endregion

        #region Main Processing

        public async Task ProcessSeries(string libraryName, string serieName, CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);
            Series? series = toothPickContext.Series.FirstOrDefault(series => series.LibraryName.Equals(libraryName) && series.Name.Equals(serieName));

            if (series == null)
                return;

            try
            {
                CancellationTokenSource serieStoppingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, new());
                CancellationToken serieStoppingToken = serieStoppingTokenSource.Token;

                foreach (Location location in series.Locations)
                {
                    if (serieStoppingToken.IsCancellationRequested)
                        return;

                    string userAgent = (await toothPickContext.Settings.GetSettingAsync("UserAgent", serieStoppingToken)).Value;
                    string limitRate = (await toothPickContext.Settings.GetSettingAsync("TotalRateLimitMBPS", serieStoppingToken)).Value;
                    float? limitRateMBPS = float.TryParse(limitRate, out float parsedSettingCount) ? parsedSettingCount : null;

                    OptionSet optionSet = new()
                    {
                        NoCacheDir = true,
                        RemoveCacheDir = true,
                        AddHeaders = $"User-Agent:{userAgent}",
                        MatchFilters = "!is_live"
                    };
                    if (!string.IsNullOrWhiteSpace(location.DownloadFormat))
                        optionSet.Format = location.DownloadFormat;
                    if (!string.IsNullOrWhiteSpace(location.Cookies))
                    {
                        string cookiesPath = (await toothPickContext.Settings.GetSettingAsync("CookiesPath", serieStoppingToken)).Value;
                        FileInfo cookiesFile = new(Path.Combine(cookiesPath, location.Cookies));
                        if (cookiesFile.Exists)
                            optionSet.Cookies = cookiesFile.FullName;
                    }
                    if (!string.IsNullOrWhiteSpace(location.MatchFilters))
                        optionSet.MatchFilters += $" & {location.MatchFilters}";
                    if (limitRateMBPS.HasValue)
                    {
                        int downloaderCount = await ProcessorMax("ParallelDownloads", toothPickContext, serieStoppingToken);
                        optionSet.LimitRate = (long)(limitRateMBPS.Value / downloaderCount * 1024 * 1024);
                    }

                    Setting defaultFetchCountSetting = await toothPickContext.Settings.GetSettingAsync("DefaultFetchCount", serieStoppingToken);
                    int defaultFetchCount = int.TryParse(defaultFetchCountSetting.Value, out int parsedDefaultFetchCount) ? parsedDefaultFetchCount : Defaults.DefaultFetchCount;

                    Setting newSeriesFetchCountOverrideSetting = await toothPickContext.Settings.GetSettingAsync("NewSeriesFetchCountOverride", serieStoppingToken);
                    int newSeriesFetchCountOverride = int.TryParse(newSeriesFetchCountOverrideSetting.Value, out int parsedNewSeriesFetchCountOverride) ? parsedNewSeriesFetchCountOverride : Defaults.NewSeriesFetchCountOverride;

                    bool isNewSeries = location.Series?.Medias.Count == 0;

                    int itemsToFetch = defaultFetchCount;
                    itemsToFetch = isNewSeries ? newSeriesFetchCountOverride : (location.FetchCount ?? itemsToFetch);
                    optionSet.PlaylistItems = location.ReverseFetch ? $"-1:-{itemsToFetch}:-1" : $"1:{itemsToFetch}";

                    if (itemsToFetch > 0)
                    {
                        RunResult<IEnumerable<VideoData>> runResult = await PlaylistDataFetcher.RunPlaylistDataFetch(
                            location.Url,
                            true,
                            async (VideoData videoData) =>
                            {
                                if (!series.Medias.Any(item => item.Url.Equals(videoData.WebpageUrl)) || DownloadsService.Downloads.Any(mediaDownload => mediaDownload.Value.Media.Url.Equals(videoData.WebpageUrl)))
                                    await MediaMetadataCallback(videoData, series, location, optionSet, isNewSeries, serieStoppingTokenSource);
                            },
                            async (string errorData) =>
                            {
                                await ErrorDataCallback(errorData, series, location, serieStoppingToken);
                            },
                            ct: serieStoppingToken,
                            overrideOptions: optionSet);
                    }
                }

                await SaveSeriesMetadata(series, toothPickContext, serieStoppingToken);

            }
            catch (Exception exception) when (exception is OperationCanceledException || exception is TaskCanceledException)
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

                StatusService.IncrementProcessingPercent();
                foreach (Func<Task> updateDelegate in StatusService.UpdateProcessingDelegates.Values)
                    await updateDelegate.Invoke();

                await toothPickContext.DisposeAsync();
            }
        }

        private async Task MediaMetadataCallback(VideoData videoData, Series series, Location location, OptionSet optionSet, bool isNewSeries, CancellationTokenSource cancellationTokenSource)
        {
            if (StatusService.Statuses.TryAdd(series, new Status
            {
                Series = series,
                SerieCancellationTokenSource = cancellationTokenSource
            }))
            {
                foreach (Func<Task> updateDelegate in StatusService.UpdateStatusesDelegates.Values)
                    await updateDelegate.Invoke();
            }

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            try
            {
                using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(cancellationToken);
                string videoUrl = videoData.Url;

                RunResult<VideoData> runResult = await MediaDataFetcher.RunVideoDataFetch(videoUrl, false, optionSet, cancellationToken);
                if (runResult.Success && runResult.Data != null)
                    videoData = runResult.Data;
                else
                {
                    await ErrorDataCallback(string.Join("; ", runResult.ErrorOutput), series, location, cancellationToken);
                    return;
                }

                Media newMedia = new()
                {
                    LibraryName = location.LibraryName,
                    SeriesName = location.SeriesName,
                    Url = videoUrl,
                    Title = videoData.Title ?? string.Empty,
                    Description = videoData.Description ?? string.Empty,
                    Duration = videoData.Duration,
                    ThumbnailLocation = videoData.Thumbnail ?? videoData.Thumbnails?.OrderByDescending(thumbnail => thumbnail.Height).FirstOrDefault()?.Url ?? string.Empty,
                    DatePublished = videoData.UploadDate ?? videoData.ReleaseDate ?? DateTime.Now,
                    SeasonNumber = decimal.TryParse(videoData.SeasonNumber, out decimal parsedSeasonNumber) ? Convert.ToInt32(Math.Floor(parsedSeasonNumber)) : null,
                    EpisodeNumber = decimal.TryParse(videoData.EpisodeNumber, out decimal parsedEpisodeNumber) ? Convert.ToInt32(Math.Floor(parsedEpisodeNumber)) : null
                };

                if (!newMedia.SeasonNumber.HasValue)
                {
                    int? newSeasonNumber = GetSeasonNumber(newMedia.Title);
                    if (newSeasonNumber.HasValue)
                        newMedia.SeasonNumber = newSeasonNumber.Value;
                    else
                        newMedia.SeasonNumber = 1;
                }

                if (!newMedia.EpisodeNumber.HasValue)
                {
                    int? newEpisodeNumber = GetEpisodeNumber(newMedia.Title);
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

                Setting downloadEnabledSetting = await toothPickContext.Settings.GetSettingAsync("DownloadEnabled", cancellationToken);
                bool downloadEnabled = !bool.TryParse(downloadEnabledSetting.Value, out bool parsedDownloadEnabled) || parsedDownloadEnabled;

                Setting newSeriesDownloadEnabledOverrideSetting = await toothPickContext.Settings.GetSettingAsync("NewSeriesDownloadEnabledOverride", cancellationToken);
                bool newSeriesDownloadEnabledOverride = !bool.TryParse(newSeriesDownloadEnabledOverrideSetting.Value, out bool parsedNewSeriesDownloadEnabledOverride) || parsedNewSeriesDownloadEnabledOverride;

                if (isNewSeries ? newSeriesDownloadEnabledOverride : downloadEnabled)
                {
                    if (DownloadsService.Downloads.TryAdd(newMedia, new Download
                    {
                        Media = newMedia,
                        DownloadCancellationTokenSource = mediaDownload.DownloadCancellationTokenSource
                    }))
                    {
                        foreach (Func<Task> updateDelegate in DownloadsService.UpdateDelegates.Values)
                            await updateDelegate.Invoke();
                    }

                    mediaDownload.DownloadCancellationTokenSource.Token.Register(async () =>
                    {
                        await RemoveMediaFromDownloadService(newMedia);
                    });

                    await DownloadMedia(mediaDownload, cancellationToken);
                }
                else
                {
                    toothPickContext.Media.Add(mediaDownload.Media);
                    await toothPickContext.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception exception) when (exception is OperationCanceledException || exception is TaskCanceledException)
            {
                await GotifyService.PushMessage("ToothPick Debug", $"Canceled processing of {series.LibraryName} series {series.Name} at {location.Name}.", LogLevel.Debug, cancellationToken);
                Logger.LogInformation("Debug ({dateTime}) - Canceled processing of {libraryName} series {serieName}.", DateTime.Now, series.LibraryName, series.Name);
            }
            catch (Exception exception)
            {
                await GotifyService.PushMessage("ToothPick Error", $"Error during metadata retrieval of {series.LibraryName} series {series.Name} at {location.Name}: {exception.Message + Environment.NewLine + exception.StackTrace}", LogLevel.Error, cancellationToken);
                Logger.LogError("Error ({dateTime}) - Error during metadata retrieval of {libraryName} series {seriesName} at {locationName}: {exception}", DateTime.Now, series.LibraryName, series.Name, location.Name, exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private async Task ErrorDataCallback(string errorData, Series series, Location location, CancellationToken serieStoppingToken)
        {
            try
            {
                if (errorData.StartsWith("error", StringComparison.InvariantCultureIgnoreCase))
                {
                    string convertedUrl = $"{location.Url}&mediaKey={errorData.GetKey()}";

                    if (series.Medias.Any(item => item.Url.Equals(convertedUrl)))
                        return;

                    await RegisterErrorDownload(new()
                    {
                        LibraryName = location.LibraryName,
                        Library = location.Library,
                        SeriesName = location.SeriesName,
                        Series = location.Series,
                        Url = convertedUrl,
                        Description = errorData,
                        DatePublished = DateTime.Now,

                    }, errorData);
                    await GotifyService.PushMessage("ToothPick Error", $"Error while fetching data for {location.LibraryName} series {location.SeriesName} location: {location.Url}; {errorData}", LogLevel.Error, serieStoppingToken);
                    Logger.LogInformation("Error ({dateTime}) - Error while fetching data for {libraryName} series {serieName} location: {location}; {errors}", DateTime.Now, location.LibraryName, location.SeriesName, location.Url, errorData);
                }
                else if (errorData.StartsWith("warning", StringComparison.InvariantCultureIgnoreCase))
                {
                    await GotifyService.PushMessage("ToothPick Warning", $"Warning while fetching data for {location.LibraryName} series {location.SeriesName} location: {location.Url}; {errorData}", LogLevel.Warning, serieStoppingToken);
                    Logger.LogInformation("Warning ({dateTime}) - Warning while fetching data for {libraryName} series {serieName} location: {location}; {errors}", DateTime.Now, location.LibraryName, location.SeriesName, location.Url, errorData);
                }
            }
            catch (Exception exception)
            {
                await GotifyService.PushMessage("ToothPick Error", $"Error during saving of error data: \"{errorData}\": {exception.Message + Environment.NewLine + exception.StackTrace}", LogLevel.Error, serieStoppingToken);
                Logger.LogError("Error ({dateTime}) - Error during saving of eror data: \"{errorData}\": {exception}", DateTime.Now, errorData, exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        #endregion

        #region Video Processing

        private async Task DownloadMedia(MediaDownload mediaDownload, CancellationToken stoppingToken)
        {
            Media media = mediaDownload.Media;
            stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, mediaDownload.DownloadCancellationTokenSource.Token).Token;

            try
            {
                using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(stoppingToken);

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

                mediaDownload.OptionSet.EmbedSubs = true;
                mediaDownload.OptionSet.SubLangs = "all";
                mediaDownload.OptionSet.RemuxVideo = "mkv";
                mediaDownload.OptionSet.TrimFilenames = 150;

                RunResult<string> runResult = await MediaDownloader.RunVideoDownload(
                    media.Url,
                    format: mediaDownload.OptionSet.Format,
                    mergeFormat: DownloadMergeFormat.Mkv,
                    progress: downloadProgress,
                    ct: stoppingToken,
                    overrideOptions: mediaDownload.OptionSet);

                await ProcessRunResult(media, runResult, stoppingToken);
            }
            catch (Exception exception) when (exception is OperationCanceledException || exception is TaskCanceledException)
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

                    toothPickContext.Media.Add(entity: media);
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
                await GotifyService.PushMessage("ToothPick Error", $"Could not download {media.LibraryName} series {media.SeriesName} media: {media.Url}; {string.Join(Environment.NewLine, runResult.ErrorOutput)}", LogLevel.Error, stoppingToken);
                Logger.LogError("Error ({dateTime}) - Could not download {libraryName} series {serieName} media: {mediaLocation}; {errors}", DateTime.Now, media.LibraryName, media.SeriesName, media.Url, string.Join(Environment.NewLine, runResult.ErrorOutput));
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
                    File.SetLastWriteTime(destinationFile, media.DatePublished ?? DateTime.Now);
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
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            string logFilterTokensString = (await toothPickContext.Settings.GetSettingAsync("LogFilterTokens")).Value;
            IEnumerable<string> LogFilterTokens = logFilterTokensString.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (LogFilterTokens.Any(token => errorMessage.Contains(token, StringComparison.InvariantCultureIgnoreCase)))
                return;

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

        private static async Task SaveSeriesMetadata(Series series, ToothPickContext toothPickContext, CancellationToken stoppingToken)
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
            Path.Combine(path, $"{new string(Path.GetFileNameWithoutExtension(fileName).SkipLast(removeCount).ToArray())}{append} {(seasonNumber != null ? $"S{seasonNumber:00}" : string.Empty)}{(episodeNumber != null ? $"E{episodeNumber:00}" : string.Empty)}{extension}");

        private static int? GetSeasonNumber(string title)
        {
            Match? match = Helpers.SeasonRegex().Matches(title).LastOrDefault();
            if (match != null && match.Success && match.Groups.Values.Count() >= 2 && int.TryParse(match.Groups.Values.ElementAt(1).Value, out int seasonNumber))
                return seasonNumber;
            else
                return null;
        }

        private static int? GetEpisodeNumber(string title)
        {
            Match? match = Helpers.EpisodeRegex().Match(title);
            if (match != null && match.Success && match.Groups.Values.Count() >= 2 && int.TryParse(match.Groups.Values.ElementAt(1).Value, out int episodeNumber))
                return episodeNumber;
            else
                return null;
        }

        private static async Task DownloadFile(ToothPickContext toothPickContext, string fileUri, string downloadLocation, CancellationToken stoppingToken)
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