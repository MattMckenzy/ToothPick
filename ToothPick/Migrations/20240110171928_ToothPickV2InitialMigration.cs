using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    /// <inheritdoc />
    public partial class ToothPickV2InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    LibraryName = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailLocation = table.Column<string>(type: "TEXT", nullable: false),
                    PosterLocation = table.Column<string>(type: "TEXT", nullable: false),
                    BannerLocation = table.Column<string>(type: "TEXT", nullable: false),
                    LogoLocation = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => new { x.LibraryName, x.Name });
                    table.ForeignKey(
                        name: "FK_Series_Libraries_LibraryName",
                        column: x => x.LibraryName,
                        principalTable: "Libraries",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LibraryName = table.Column<string>(type: "TEXT", nullable: false),
                    SeriesName = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    FetchCount = table.Column<int>(type: "INTEGER", nullable: true),
                    MatchFilters = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadFormat = table.Column<string>(type: "TEXT", nullable: false),
                    Cookies = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => new { x.LibraryName, x.SeriesName, x.Name });
                    table.ForeignKey(
                        name: "FK_Locations_Libraries_LibraryName",
                        column: x => x.LibraryName,
                        principalTable: "Libraries",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Locations_Series_LibraryName_SeriesName",
                        columns: x => new { x.LibraryName, x.SeriesName },
                        principalTable: "Series",
                        principalColumns: ["LibraryName", "Name"],
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    LibraryName = table.Column<string>(type: "TEXT", nullable: false),
                    SeriesName = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    EpisodeNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    Duration = table.Column<float>(type: "REAL", nullable: true),
                    ThumbnailLocation = table.Column<string>(type: "TEXT", nullable: false),
                    DatePublished = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => new { x.LibraryName, x.SeriesName, x.Url });
                    table.ForeignKey(
                        name: "FK_Media_Libraries_LibraryName",
                        column: x => x.LibraryName,
                        principalTable: "Libraries",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Media_Series_LibraryName_SeriesName",
                        columns: x => new { x.LibraryName, x.SeriesName },
                        principalTable: "Series",
                        principalColumns: ["LibraryName", "Name"],
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Libraries");
        }
    }
}
