using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    public partial class UpdatedDefaultSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-youtube.com" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-youtube.com/playlist" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultKeyRegex-1-youtube.com/playlist", "youtube\\.com\\/watch\\?v=(.*)&list" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultKeyRegex-2-youtube.com", "youtube\\.com\\/watch\\?v=(.*)&list" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-1-youtube.com/playlist" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-2-youtube.com" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultKeyRegex-youtube.com", "youtube\\.com\\/watch\\?v=(.*)&list" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[] { "Default", "DefaultKeyRegex-youtube.com/playlist", "youtube\\.com\\/watch\\?v=(.*)&list" });
        }
    }
}
