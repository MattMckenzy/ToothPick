namespace ToothPick.Services
{
    /// <summary>
    /// Extends the rest service caller for a singleton-designed client token call.
    /// </summary>
    /// <remarks>
    /// Default constructor.
    /// </remarks>
    /// <param name="restServiceProvider">An instance of the service provider used for this caller.</param>
    /// <param name="httpClient">An instance of a configured HttpClient.</param>
    public sealed class StaticTokenCaller<T>(T restServiceProvider, HttpClient httpClient) : IRestServiceCaller where T : IRestServiceProvider
    {
        private readonly T _restServiceProvider = restServiceProvider;
        private readonly HttpClient _httpClient = httpClient;

        /// <summary>
        /// Builds and returns a base request message containing proper configuration and authentication.
        /// </summary>
        /// <returns>The base HttpRequestMessage.</returns>
        async Task<HttpRequestMessage> IRestServiceCaller.GetBaseRequestMessage(string? credential, Func<string, string, Task<string>>? accessTokenDelegate)
        {
            HttpRequestMessage returningHttpRequestMessage = new()
            {
                RequestUri = await _restServiceProvider.GetServiceUri()
            };

            returningHttpRequestMessage.Headers.Add(await _restServiceProvider.GetHeader() ?? throw new InvalidOperationException("Header must be provided for the static token caller!"), await _restServiceProvider.GetToken());

            return returningHttpRequestMessage;
        }

        public Task RefreshAccessToken(string _, Func<string, string, Task<string>> __)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends the given http request message with configurated request message..
        /// </summary>
        /// <returns>The response message.</returns>
        public async Task<HttpResponseMessage> SendRequest(HttpRequestMessage httpRequestMessage)
        {
            return await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
        }
    }
}