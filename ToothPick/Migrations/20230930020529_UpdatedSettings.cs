using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ToothPick.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                table: "Settings");

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Default", "DownloadPath" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Default", "Environment" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Default", "GotifyAppId" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Default", "GotifyAppToken" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Default", "GotifyClientToken" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Default", "GotifyHeader" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Default", "GotifyUri" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Debug", "NewSeriesFetchCountOverride" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Production", "NewSeriesFetchCountOverride" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Debug", "ParallelDownloads" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Production", "ParallelDownloads" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Debug", "ParallelFetch" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Production", "ParallelFetch" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Debug", "ScanDelayMinutes" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Production", "ScanDelayMinutes" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Default", "ToothPickEnabled" });

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyColumnTypes: new[] { "TEXT", "TEXT" },
                keyValues: new object[] { "Default", "UserAgent" });

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "Settings");

            migrationBuilder.AddColumn<string>(
                name: "MatchFilters",
                table: "Locations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                table: "Settings",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "MatchFilters",
                table: "Locations");

            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                table: "Settings",
                columns: new[] { "Name", "Environment" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name", "Value" },
                values: new object[,]
                {
                    { "Default", "DownloadPath", "/root/ToothPick/Media" },
                    { "Default", "Environment", "Debug" },
                    { "Default", "GotifyAppId", "" },
                    { "Default", "GotifyAppToken", "" },
                    { "Default", "GotifyClientToken", "" },
                    { "Default", "GotifyHeader", "X-Gotify-Key" },
                    { "Default", "GotifyUri", "" },
                    { "Debug", "NewSeriesFetchCountOverride", "0" },
                    { "Production", "NewSeriesFetchCountOverride", "0" },
                    { "Debug", "ParallelDownloads", "4" },
                    { "Production", "ParallelDownloads", "4" },
                    { "Debug", "ParallelFetch", "4" },
                    { "Production", "ParallelFetch", "4" },
                    { "Debug", "ScanDelayMinutes", "0" },
                    { "Production", "ScanDelayMinutes", "5" },
                    { "Default", "ToothPickEnabled", "True" },
                    { "Default", "UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" }
                });
        }
    }
}
