using Microsoft.EntityFrameworkCore.Migrations;

namespace ToothPick.Migrations
{
    public partial class AddedSettings2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "ChromeDriverLocation" },
                column: "Value",
                value: "resources");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "ChromeDriverLocation" },
                column: "Value",
                value: "Resources");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "DownloadPath" },
                column: "Value",
                value: "videos");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "DownloadPath" },
                column: "Value",
                value: "/srv/dev-disk-by-label-cctv/Media");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "Environment" },
                column: "Value",
                value: "Debug");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "FFMPEGLocation" },
                column: "Value",
                value: "resources\\ffmpeg\\ffmpeg.exe");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "FFMPEGLocation" },
                column: "Value",
                value: "Resources/ffmpeg/ffmpeg");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "ParallelChromes" },
                column: "Value",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "ParallelChromes" },
                column: "Value",
                value: "4");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "ParallelDownloads" },
                column: "Value",
                value: "1");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "ParallelDownloads" },
                column: "Value",
                value: "4");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "ScanDelayMinutes" },
                column: "Value",
                value: "0");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "ScanDelayMinutes" },
                column: "Value",
                value: "5");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "SkipNewSeries" },
                column: "Value",
                value: "True");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "SkipNewSeries" },
                column: "Value",
                value: "True");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "ToothPickEnabled" },
                column: "Value",
                value: "False");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "UserAgent" },
                column: "Value",
                value: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Debug", "YoutubeDLLocation" },
                column: "Value",
                value: "resources\\youtube-dl.exe");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Production", "YoutubeDLLocation" },
                column: "Value",
                value: "Resources/youtube-dl");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "Settings");
        }
    }
}
