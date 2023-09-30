using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ToothPick.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "ChromeDriverLocation" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-1-youtube.com/playlist" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-2-youtube.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-3-crunchyroll.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-4-twitch.tv" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-5-roosterteeth.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-1-youtube.com/playlist" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-2-youtube.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-3-crunchyroll.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-4-crunchyroll.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-5-twitch.tv" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-6-roosterteeth.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "FFMPEGLocation" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "FixEpisodeNumbers" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "ParallelChromes" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "ParallelChromes" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "SkipNewSeries" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "SkipNewSeries" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "ToothPickId" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "ToothPickToken" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "ToothpickClientToken" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "YoutubeDLLocation" });

            migrationBuilder.DropColumn(
                name: "MediaXPath",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "KeyRegex",
                table: "Locations");

            migrationBuilder.AddColumn<string>(
                name: "Cookies",
                table: "Locations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DownloadFormat",
                table: "Locations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FetchCount",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                columns: new[] { "LibraryName", "SerieName", "Url" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DownloadPath" },
                column: "Value",
                value: "/root/ToothPick/Media");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "ParallelDownloads" },
                column: "Value",
                value: "4");

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[,]
                {
                    { "Default", "GotifyAppId", "" },
                    { "Default", "GotifyAppToken", "" },
                    { "Default", "GotifyClientToken", "" },
                    { "Debug", "NewSeriesFetchCountOverride", "0" },
                    { "Production", "NewSeriesFetchCountOverride", "0" },
                    { "Debug", "ParallelFetch", "4" },
                    { "Production", "ParallelFetch", "4" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "GotifyAppId" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "GotifyAppToken" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "GotifyClientToken" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "NewSeriesFetchCountOverride" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "NewSeriesFetchCountOverride" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "ParallelFetch" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "ParallelFetch" });

            migrationBuilder.DropColumn(
                name: "Cookies",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "DownloadFormat",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "FetchCount",
                table: "Locations");

            migrationBuilder.AddColumn<string>(
                name: "MediaXPath",
                table: "Locations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KeyRegex",
                table: "Locations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                columns: new[] { "LibraryName", "SerieName", "Url", "MediaXPath", "KeyRegex" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DownloadPath" },
                column: "Value",
                value: "/Media");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "ParallelDownloads" },
                column: "Value",
                value: "1");

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[,]
                {
                    { "Default", "ChromeDriverLocation", "resources" },
                    { "Default", "DefaultKeyRegex-1-youtube.com/playlist", "youtube\\.com\\/watch\\?v=(.+)&list" },
                    { "Default", "DefaultKeyRegex-2-youtube.com", "youtube\\.com\\/watch\\?v=(.+)" },
                    { "Default", "DefaultKeyRegex-3-crunchyroll.com", "crunchyroll\\.com\\/.*\\/.*-([0-9]+)" },
                    { "Default", "DefaultKeyRegex-4-twitch.tv", "twitch\\.tv\\/videos\\/([0-9]+)" },
                    { "Default", "DefaultKeyRegex-5-roosterteeth.com", "roosterteeth\\.com\\/watch\\/(.+)" },
                    { "Default", "DefaultXPath-1-youtube.com/playlist", "//div[@id='contents']/ytd-playlist-video-renderer/div/div/ytd-thumbnail/a" },
                    { "Default", "DefaultXPath-2-youtube.com", "//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a" },
                    { "Default", "DefaultXPath-3-crunchyroll.com", "//li[contains(@class, 'season')]/a[@title='PLACEHOLDER']/following-sibling::ul[1]/li/div/a" },
                    { "Default", "DefaultXPath-4-crunchyroll.com", "//li[contains(@class, 'season')]/ul/li/div/a" },
                    { "Default", "DefaultXPath-5-twitch.tv", "//div[@data-test-selector='content']/div/div/div/article/div/div/div/a[@data-a-target='preview-card-image-link']" },
                    { "Default", "DefaultXPath-6-roosterteeth.com", "//div[contains(@class, 'episode-grid-container')]/div/div/div/div/a" },
                    { "Default", "FFMPEGLocation", "resources/ffmpeg" },
                    { "Default", "FixEpisodeNumbers", "False" },
                    { "Debug", "ParallelChromes", "1" },
                    { "Production", "ParallelChromes", "4" },
                    { "Debug", "SkipNewSeries", "True" },
                    { "Production", "SkipNewSeries", "True" },
                    { "Default", "ToothPickId", "" },
                    { "Default", "ToothPickToken", "" },
                    { "Default", "ToothpickClientToken", "" },
                    { "Default", "YoutubeDLLocation", "resources/yt-dlp" }
                });
        }
    }
}
