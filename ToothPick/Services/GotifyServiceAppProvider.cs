namespace ToothPick.Services
{
    public class GotifyServiceAppProvider(IDbContextFactory<ToothPickContext> toothPickContextFactory) : IRestServiceProvider
    {
        private IDbContextFactory<ToothPickContext> ToothPickContextFactory { get; set; } = toothPickContextFactory;

        public async Task<string> GetHeader()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            string value = (await toothPickContext.Settings.GetSettingAsync("GotifyHeader")).Value;

            return value;
        }

        public Task<string> GetScope()
        {
            throw new NotImplementedException();
        }

        public async Task<Uri> GetServiceUri()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            string value = (await toothPickContext.Settings.GetSettingAsync("GotifyUri")).Value;

            return new Uri(value);
        }

        public async Task<string> GetToken()
        {
            using ToothPickContext toothPickContext = await ToothPickContextFactory.CreateDbContextAsync();

            string value = (await toothPickContext.Settings.GetSettingAsync("GotifyAppToken")).Value;

            return value;
        }
    }
}