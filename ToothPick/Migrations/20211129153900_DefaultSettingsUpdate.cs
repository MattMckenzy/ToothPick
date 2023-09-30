using Microsoft.EntityFrameworkCore.Migrations;

namespace ToothPick.Migrations
{
    public partial class DefaultSettingsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "XPath-1-youtube.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "XPath-1-youtube.com/playlist" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultXPath-1-youtube.com/playlist", "//div[@id='contents']/ytd-playlist-video-renderer/div/div/ytd-thumbnail/a" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultXPath-2-youtube.com", "//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultKeyRegex-youtube.com/playlist", "youtube\\.com\\/watch\\?v=(.*)&list" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultKeyRegex-youtube.com", "youtube\\.com\\/watch\\?v=(.*)&list" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-youtube.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-youtube.com/playlist" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-1-youtube.com/playlist" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-2-youtube.com" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "XPath-1-youtube.com/playlist", "//div[@id='contents']/ytd-playlist-video-renderer/div/div/ytd-thumbnail/a" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "XPath-1-youtube.com", "//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a" });
        }
    }
}
