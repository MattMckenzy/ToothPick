using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePublished",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "EpisodeCount",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "SeasonCount",
                table: "Series");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatePublished",
                table: "Series",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EpisodeCount",
                table: "Series",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonCount",
                table: "Series",
                type: "INTEGER",
                nullable: true);
        }
    }
}
