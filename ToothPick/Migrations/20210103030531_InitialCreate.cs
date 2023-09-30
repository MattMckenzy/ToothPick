using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ToothPick.Migrations
{
    public partial class InitialCreate : Migration
    {
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
                name: "Series",
                columns: table => new
                {
                    LibraryName = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
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
                    SerieName = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => new { x.LibraryName, x.SerieName, x.Url });
                    table.ForeignKey(
                        name: "FK_Locations_Libraries_LibraryName",
                        column: x => x.LibraryName,
                        principalTable: "Libraries",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Locations_Series_LibraryName_SerieName",
                        columns: x => new { x.LibraryName, x.SerieName },
                        principalTable: "Series",
                        principalColumns: new[] { "LibraryName", "Name" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medias",
                columns: table => new
                {
                    LibraryName = table.Column<string>(type: "TEXT", nullable: false),
                    SerieName = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<float>(type: "REAL", nullable: true),
                    ThumbnailLocation = table.Column<string>(type: "TEXT", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medias", x => new { x.LibraryName, x.SerieName, x.Location });
                    table.ForeignKey(
                        name: "FK_Medias_Libraries_LibraryName",
                        column: x => x.LibraryName,
                        principalTable: "Libraries",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Medias_Series_LibraryName_SerieName",
                        columns: x => new { x.LibraryName, x.SerieName },
                        principalTable: "Series",
                        principalColumns: new[] { "LibraryName", "Name" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Medias");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Libraries");
        }
    }
}
