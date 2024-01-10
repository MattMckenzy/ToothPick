global using System;
global using System.ComponentModel.DataAnnotations;
global using System.IO;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Collections.Specialized;
global using System.Globalization;
global using System.Linq;
global using System.Net;
global using System.Net.Http;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Web;
global using System.Xml.Linq;
global using System.Linq.Dynamic.Core;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.JSInterop;

global using ToothPick.Components;
global using ToothPick.Exceptions;
global using ToothPick.Extensions;
global using ToothPick.Models;
global using ToothPick.Services;

global using Newtonsoft.Json;

WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(args);

webApplicationBuilder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

// Add services to the container.1
webApplicationBuilder.Services.AddDbContextFactory<ToothPickContext>();

webApplicationBuilder.Services.AddRazorPages();
webApplicationBuilder.Services.AddServerSideBlazor();

webApplicationBuilder.Services.AddHttpClient<StaticTokenCaller<GotifyServiceClientProvider>>();
webApplicationBuilder.Services.AddHttpClient<StaticTokenCaller<GotifyServiceAppProvider>>();
webApplicationBuilder.Services.AddSingleton<GotifyServiceClientProvider>();
webApplicationBuilder.Services.AddSingleton<GotifyServiceAppProvider>();
webApplicationBuilder.Services.AddSingleton<GotifyService>();
webApplicationBuilder.Services.AddSingleton<DownloadsService>();
webApplicationBuilder.Services.AddSingleton<StatusService>();
webApplicationBuilder.Services.AddHostedService<ToothPickHostedService>();


WebApplication webApplication = webApplicationBuilder.Build();

using (IServiceScope scope = webApplication.Services.CreateAsyncScope())
{
    ToothPickContext toothPickContext = scope.ServiceProvider.GetRequiredService<ToothPickContext>();
    
    toothPickContext.Database.Migrate();
    await toothPickContext.Settings.PopulateDefaultsAsync();
}


// Configure the HTTP request pipeline.
if (!webApplication.Environment.IsDevelopment())
{
    webApplication.UseDeveloperExceptionPage();
}
else
{
    webApplication.UseExceptionHandler("/Error");
}

webApplication.UseStaticFiles();

webApplication.UseRouting();

webApplication.UseAuthentication();
webApplication.UseAuthorization();

webApplication.MapControllers();
webApplication.MapBlazorHub();
webApplication.MapFallbackToPage("/_Host");

webApplication.Run();