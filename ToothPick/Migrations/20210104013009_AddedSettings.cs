using Microsoft.EntityFrameworkCore.Migrations;

namespace ToothPick.Migrations
{
    public partial class AddedSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Environment = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => new { x.Name, x.Environment });
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Default", "Environment" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Production", "ScanDelayMinutes" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Debug", "ScanDelayMinutes" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Production", "DownloadPath" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Debug", "DownloadPath" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Default", "UserAgent" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Production", "YoutubeDLLocation" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Debug", "YoutubeDLLocation" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Debug", "SkipNewSeries" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Production", "FFMPEGLocation" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Production", "ChromeDriverLocation" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Debug", "ChromeDriverLocation" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Production", "ParallelDownloads" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Debug", "ParallelDownloads" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Production", "ParallelChromes" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Debug", "ParallelChromes" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Default", "ToothPickEnabled" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Debug", "FFMPEGLocation" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Environment", "Name" },
                values: new object[] { "Production", "SkipNewSeries" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
