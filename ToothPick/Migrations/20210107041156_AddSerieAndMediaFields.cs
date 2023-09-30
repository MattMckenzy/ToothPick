using Microsoft.EntityFrameworkCore.Migrations;

namespace ToothPick.Migrations
{
    public partial class AddSerieAndMediaFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "Medias",
                newName: "DatePublished");

            migrationBuilder.AddColumn<int>(
                name: "EpisodeCount",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeasonCount",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EpisodeNumber",
                table: "Medias",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeasonNumber",
                table: "Medias",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EpisodeCount",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "SeasonCount",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "EpisodeNumber",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "SeasonNumber",
                table: "Medias");

            migrationBuilder.RenameColumn(
                name: "DatePublished",
                table: "Medias",
                newName: "TimeStamp");
        }
    }
}
