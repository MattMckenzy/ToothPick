using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    public partial class FixedDefaultXPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-2-youtube.com" },
                column: "Value",
                value: "//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultXPath-2-youtube.com" },
                column: "Value",
                value: "//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a//div[@id='items']/ytd-grid-video-renderer/div/ytd-thumbnail/a");
        }
    }
}
