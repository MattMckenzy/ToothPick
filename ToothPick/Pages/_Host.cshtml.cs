using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ToothPick.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class HostModel(IDbContextFactory<ToothPickContext> dbContextFactory) : PageModel
    {
        private IDbContextFactory<ToothPickContext> DbContextFactory { get; set; } = dbContextFactory;

        public string ColorTheme = "dark";

        public async void OnGet()
        {
            ToothPickContext toothPickContext = await DbContextFactory.CreateDbContextAsync();
            ColorTheme = (await toothPickContext.Settings.GetSettingAsync("ColorTheme")).Value.ToLower();                    
        }
    }
}
