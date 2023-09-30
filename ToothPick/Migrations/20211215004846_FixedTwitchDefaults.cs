using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    public partial class FixedTwitchDefaults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-4-twitch.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-5-twitch.com" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-1-youtube.com/playlist" },
                column: "Value",
                value: "youtube\\.com\\/watch\\?v=(.+)&list");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-2-youtube.com" },
                column: "Value",
                value: "youtube\\.com\\/watch\\?v=(.+)");

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultKeyRegex-4-twitch.tv", "twitch\\.tv\\/videos\\/([0-9]+)" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultXPath-5-twitch.tv", "//div[@data-test-selector='content']/div/div/div/article/div/div/div/a[@data-a-target='preview-card-image-link']" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-4-twitch.tv" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-5-twitch.tv" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-1-youtube.com/playlist" },
                column: "Value",
                value: "youtube\\.com\\/watch\\?v=(.*)&list");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-2-youtube.com" },
                column: "Value",
                value: "youtube\\.com\\/watch\\?v=(.*)");

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultKeyRegex-4-twitch.com", "twitch\\.tv\\/videos\\/([0-9]+)" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultXPath-5-twitch.com", "//div[@data-test-selector='content']/div/div/div/article/div/div/div/a[@data-a-target='preview-card-image-link']" });
        }
    }
}
