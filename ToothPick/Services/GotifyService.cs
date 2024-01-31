using WatsonWebsocket;

namespace ToothPick.Services
{
    public class GotifyService(IDbContextFactory<ToothPickContext> toothPickContextFactory, StaticTokenCaller<GotifyServiceClientProvider> gotifyClientCaller, StaticTokenCaller<GotifyServiceAppProvider> gotifyAppCaller, ILogger<GotifyService> logger)
    {
        private IDbContextFactory<ToothPickContext> ToothPickContextFactory { get; set; } = toothPickContextFactory;
#pragma warning disable CA1859 // Needed for interface methods.
        private readonly IRestServiceCaller GotifyClientCaller = gotifyClientCaller;
        private readonly IRestServiceCaller GotifyAppCaller = gotifyAppCaller;
#pragma warning restore CA1859
        private readonly ILogger<GotifyService> Logger = logger;

        private readonly Dictionary<int, GotifyMessage> GotifyMessages = [];
        private class GotifyMessageEventArgs { public required GotifyMessage GotifyMessage { get; set; } }
        private event EventHandler<GotifyMessageEventArgs>? OnNewGotifyMessage = null;

        public async Task PushMessage(string Title, string Message, LogLevel logLevel, CancellationToken cancellationToken = new())
        {             
            try
            {  
                using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(cancellationToken);

                string logFilterTokensString = (await toothPickContext.Settings.GetSettingAsync("LogFilterTokens", cancellationToken: cancellationToken)).Value;
                IEnumerable<string> LogFilterTokens = logFilterTokensString.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (LogFilterTokens.Any(token => 
                        Title.Contains(token, StringComparison.InvariantCultureIgnoreCase) || 
                        Message.Contains(token, StringComparison.InvariantCultureIgnoreCase)))
                    return;

                GotifyMessage newMessage;
                lock (GotifyMessages)
                {
                    newMessage = new()
                    {
                        InternalId = GotifyMessages.Count + 1,
                        Title = Title,
                        Message = Message,
                        Date = DateTime.Now,
                        Priority = GetGotifyPriority(logLevel)
                    };

                    GotifyMessages.Add(newMessage.InternalId, newMessage);
                }

                OnNewGotifyMessage?.Invoke(this, new GotifyMessageEventArgs { GotifyMessage = newMessage });

                if (await CanUseGotify(cancellationToken: cancellationToken))
                {
                    Setting gotifyLogLevelSetting = await toothPickContext.Settings.GetSettingAsync("GotifyLogLevel", cancellationToken);
                    int configuredLogLevel = int.TryParse(gotifyLogLevelSetting.Value, out int parsedLogLevel) ? parsedLogLevel : Defaults.GotifyLogLevel;

                    if (configuredLogLevel <= (int)logLevel)
                    {
                        await GotifyAppCaller.PostRequestAsync<string>(
                            "message",
                            content: JsonConvert.SerializeObject(newMessage));
                    }
                }
            }            
            catch (Exception exception) when (exception is OperationCanceledException || exception is TaskCanceledException)
            {
                Logger.LogDebug("The task was canceled while pushing a message to Gotify.");
            }
            catch (CommunicationException communicationException)
            {
                Logger.LogError("There was a problem pushing a message to Gotify: {communicationException.Message};{Environment.NewLine}Call stack: {communicationException.StackTrace}",
                    communicationException.Message,
                    Environment.NewLine,
                    communicationException.StackTrace);
            }
            catch (HttpRequestException httpRequestException)
            {
                Logger.LogError("There was a problem connecting to the configured Gotify address: {httpRequestException.Message};{Environment.NewLine}Call stack: {httpRequestException.StackTrace}",
                    httpRequestException.Message,
                    Environment.NewLine,
                    httpRequestException.StackTrace);
            }
        }

        public async Task DeleteMessage(int? gotifyMessageId = null, int? internalId = null, CancellationToken cancellationToken = new())
        {
            if (internalId.HasValue)
                GotifyMessages.Remove(internalId.Value);

            try
            {
                if (gotifyMessageId.HasValue && await CanUseGotify(cancellationToken: cancellationToken))
                    await GotifyClientCaller.DeleteRequestAsync<string>($"message/{gotifyMessageId.Value}");
            }
            catch (CommunicationException communicationException)
            {
                Logger.LogError("There was a problem deleting the message to Gotify: {communicationException.Message};{Environment.NewLine}Call stack: {communicationException.StackTrace}",
                    communicationException.Message,
                    Environment.NewLine,
                    communicationException.StackTrace);
            }
            catch (HttpRequestException httpRequestException)
            {
                Logger.LogError("There was a problem connecting to the configured Gotify address: {httpRequestException.Message};{Environment.NewLine}Call stack: {httpRequestException.StackTrace}",
                    httpRequestException.Message,
                    Environment.NewLine,
                    httpRequestException.StackTrace);
            }
        }

        public IDbContextFactory<ToothPickContext> GetToothPickContextFactory()
        {
            return ToothPickContextFactory;
        }

        public async Task DeleteMessages(IDbContextFactory<ToothPickContext> toothPickContextFactory, CancellationToken cancellationToken = new())
        {
            GotifyMessages.Clear();

            try
            {
                if (await CanUseGotify(true, cancellationToken))
                {
                    using ToothPickContext toothPickContext = await toothPickContextFactory.CreateDbContextAsync(cancellationToken);

                    Setting gotifyAppIdSetting = await toothPickContext.Settings.GetSettingAsync("GotifyAppId", cancellationToken);
                    int gotifyAppId = int.TryParse(gotifyAppIdSetting.Value, out int parsedGotifyAppId) ? parsedGotifyAppId : Defaults.GotifyAppId;

                    await GotifyClientCaller.DeleteRequestAsync<string>($"application/{gotifyAppId}/message");
                }
            }
            catch (CommunicationException communicationException)
            {
                Logger.LogError("There was a problem deleting all messages on Gotify: {communicationException.Message};{Environment.NewLine}Call stack: {communicationException.StackTrace}",
                    communicationException.Message,
                    Environment.NewLine,
                    communicationException.StackTrace);
            }
            catch (HttpRequestException httpRequestException)
            {
                Logger.LogError("There was a problem connecting to the configured Gotify address: {httpRequestException.Message};{Environment.NewLine}Call stack: {httpRequestException.StackTrace}",
                    httpRequestException.Message,
                    Environment.NewLine,
                    httpRequestException.StackTrace);
            }
        }

        public async Task<IEnumerable<GotifyMessage>> GetMessages(CancellationToken cancellationToken = new())
        {
            try
            {
                if (await CanUseGotify(true, cancellationToken))
                {
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(cancellationToken);

                    Setting gotifyAppIdSetting = await toothPickContext.Settings.GetSettingAsync("GotifyAppId", cancellationToken);
                    int gotifyAppId = int.TryParse(gotifyAppIdSetting.Value, out int parsedGotifyAppId) ? parsedGotifyAppId : Defaults.GotifyAppId;

                    CallResult<string> callResult = await GotifyClientCaller.GetRequestAsync<string>($"application/{gotifyAppId}/message");
                    if (callResult.Content != null)
                        return JsonConvert.DeserializeObject<GotifyPagedMessages>(callResult.Content)?.Messages ?? [];
                    else
                        return GotifyMessages.Values;
                }
                else
                    return GotifyMessages.Values;
            }
            catch (CommunicationException communicationException)
            {
                Logger.LogError("There was a problem getting all messages on Gotify: {communicationException.Message};{Environment.NewLine}Call stack: {communicationException.StackTrace}",
                    communicationException.Message,
                    Environment.NewLine,
                    communicationException.StackTrace);
            }
            catch (HttpRequestException httpRequestException)
            {
                Logger.LogError("There was a problem connecting to the configured Gotify address: {httpRequestException.Message};{Environment.NewLine}Call stack: {httpRequestException.StackTrace}",
                    httpRequestException.Message,
                    Environment.NewLine,
                    httpRequestException.StackTrace);
            }

            return [];
        }

        public async Task SubscribeToStream(Func<GotifyMessage, Task> callBack, CancellationToken cancellationToken = new())
        {
            if (await CanUseGotify(true, cancellationToken))
            {
                _ = Task.Run(async () =>
                {
                    using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(cancellationToken);

                    Setting gotifyAppIdSetting = await toothPickContext.Settings.GetSettingAsync("GotifyAppId", cancellationToken);
                    int gotifyAppId = int.TryParse(gotifyAppIdSetting.Value, out int parsedGotifyAppId) ? parsedGotifyAppId : Defaults.GotifyAppId;
                    string gotifyAppToken = (await toothPickContext.Settings.GetSettingAsync("GotifyAppToken", cancellationToken)).Value;

                    using WatsonWsClient socketClient =
                        new(new Uri((await GotifyClientCaller.GetEndpoint("/stream", queryParameters: new Dictionary<string, string>() { { "token", gotifyAppToken } })).ToString().Replace("http", "ws")));

                    async Task messageReceived(object? obj, MessageReceivedEventArgs messageReceivedEventArgs)
                    {
                        GotifyMessage? gotifyMessage = JsonConvert.DeserializeObject<GotifyMessage>(Encoding.UTF8.GetString(messageReceivedEventArgs.Data), new JsonSerializerSettings { CheckAdditionalContent = false });

                        if (gotifyMessage?.AppId == gotifyAppId)
                            await callBack.Invoke(gotifyMessage);
                    }

                    void reconnect(object? _, EventArgs? __)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            socketClient.Start();
                    }

                    socketClient.ServerDisconnected += new EventHandler(reconnect);
                    socketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(async (object? obj, MessageReceivedEventArgs messageReceivedEventArgs) => await messageReceived(obj, messageReceivedEventArgs));
                    reconnect(null, null);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }

                    socketClient.Stop();
                },
                CancellationToken.None);
            }
            else
            {
                OnNewGotifyMessage += async (object? sender, GotifyMessageEventArgs gotifyMessageEventArgs) =>
                {
                    await callBack.Invoke(gotifyMessageEventArgs.GotifyMessage);
                };
            }
        }

        private async Task<bool> CanUseGotify(bool checkForAppId = false, CancellationToken cancellationToken = new())
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync(cancellationToken);

            Setting gotifyAppTokenSetting = await toothPickContext.Settings.GetSettingAsync("GotifyAppToken", cancellationToken);
            Setting gotifyUriSetting = await toothPickContext.Settings.GetSettingAsync("GotifyUri", cancellationToken);
            Setting gotifyHeaderSetting = await toothPickContext.Settings.GetSettingAsync("GotifyHeader", cancellationToken);

            Setting gotifyAppIdSetting = await toothPickContext.Settings.GetSettingAsync("GotifyAppId", cancellationToken);
            int gotifyAppId = int.TryParse(gotifyAppIdSetting.Value, out int parsedLogLevel) ? parsedLogLevel : Defaults.GotifyAppId;

            return
                !string.IsNullOrWhiteSpace(gotifyAppTokenSetting.Value) &&
                !string.IsNullOrWhiteSpace(gotifyUriSetting.Value) &&
                !string.IsNullOrWhiteSpace(gotifyHeaderSetting.Value) &&
                (!checkForAppId || gotifyAppId != -1);
        }

        public int GetGotifyPriority(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical => 9,
                LogLevel.Error => 8,
                LogLevel.Warning => 5,
                LogLevel.Information => 2,
                LogLevel.Debug => 1,
                LogLevel.Trace or _ => 0
            };
        }

        public LogLevel GetLogLevel(int? priority)
        {
            return priority switch
            {
                9 => LogLevel.Critical,
                8 => LogLevel.Error,
                5 or 6 or 7 => LogLevel.Warning,
                2 or 3 or 4 => LogLevel.Information,
                1 => LogLevel.Debug,
                0 or _ => LogLevel.Trace
            };
        }
    }
}
