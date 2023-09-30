using Microsoft.EntityFrameworkCore.Migrations;

namespace ToothPick.Migrations
{
    public partial class AddedXPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "ChromeDriverLocation" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "ChromeDriverLocation" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "DownloadPath" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "DownloadPath" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "FFMPEGLocation" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "FFMPEGLocation" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "YoutubeDLLocation" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "YoutubeDLLocation" });

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Medias",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KeyRegex",
                table: "Locations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaXPath",
                table: "Locations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "ToothPickEnabled" },
                column: "Value",
                value: "True");

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "ChromeDriverLocation", "resources" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "FFMPEGLocation", "resources/ffmpeg" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "YoutubeDLLocation", "resources/youtube-dl" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DownloadPath", "/Media" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "ToothpickClientToken", "" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "ToothPickToken", "" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "GotifyUri", "" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "GotifyHeader", "X-Gotify-Key" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "FixEpisodeNumbers", "False" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "XPath-1-youtube.com/playlist", "//div[@id='contents']/ytd-playlist-video-renderer/div/div/ytd-thumbnail/a" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "XPath-1-youtube.com", "//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "ChromeDriverLocation" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DownloadPath" });

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
                keyValues: new object[] { "Default", "GotifyHeader" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "GotifyUri" });

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
                keyValues: new object[] { "Default", "XPath-1-youtube.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "XPath-1-youtube.com/playlist" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "YoutubeDLLocation" });

            migrationBuilder.DropColumn(
                name: "Key",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "KeyRegex",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "MediaXPath",
                table: "Locations");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "ToothPickEnabled" },
                column: "Value",
                value: "False");

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Debug", "ChromeDriverLocation", "resources" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Production", "ChromeDriverLocation", "Resources" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Debug", "FFMPEGLocation", "resources\\ffmpeg\\ffmpeg.exe" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Production", "FFMPEGLocation", "Resources/ffmpeg/ffmpeg" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Debug", "YoutubeDLLocation", "resources\\youtube-dl.exe" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Production", "YoutubeDLLocation", "Resources/youtube-dl" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Debug", "DownloadPath", "videos" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Production", "DownloadPath", "/srv/dev-disk-by-label-cctv/Media" });
        }
    }
}
