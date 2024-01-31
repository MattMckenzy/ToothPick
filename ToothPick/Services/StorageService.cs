using System.Security.Cryptography;

namespace ToothPick.Services
{
    public class StorageService(ProtectedLocalStorage protectedLocalStorage, GotifyService gotifyService, ILogger<StorageService> logger)
    {
        private ProtectedLocalStorage ProtectedLocalStorage { get; } = protectedLocalStorage;
        private GotifyService GotifyService { get; } = gotifyService;
        private ILogger<StorageService> Logger { get; } = logger;

        public async Task<string?> Get(string token)
        {
            try
            {
                return (await ProtectedLocalStorage.GetAsync<string>(token)).Value;
            }
            catch (CryptographicException)
            {
                await GotifyService.PushMessage("ToothPick Debug", $"Client token out of date, couldn't retrieve data from storage.", LogLevel.Debug);
                Logger.LogInformation("Debug ({dateTime}) - Client token out of date, couldn't retrieve data from storage.", DateTime.Now);        
            }
            catch (Exception exception)
            {
                await GotifyService.PushMessage("ToothPick Error", $"Error during retrieval of data from storage: {exception.Message + Environment.NewLine + exception.StackTrace}", LogLevel.Error);
                Logger.LogError("Error ({dateTime}) - Error during retrieval of data from storage: {exception}", DateTime.Now, exception.Message + Environment.NewLine + exception.StackTrace);
            }

            return null;
        }

        public async Task Set(string token, object value)
        {
            try
            {
                await ProtectedLocalStorage.SetAsync(token, value);
            }
            catch (Exception exception)
            {
                await GotifyService.PushMessage("ToothPick Error", $"Error during storage of data: {exception.Message + Environment.NewLine + exception.StackTrace}", LogLevel.Error);
                Logger.LogError("Error ({dateTime}) - Error during storage of data: {exception}", DateTime.Now, exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }
    }
}
