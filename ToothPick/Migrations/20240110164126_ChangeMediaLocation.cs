using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMediaLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Media",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Media");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "Media",
                newName: "Url");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Media",
                table: "Media",
                columns: new[] { "LibraryName", "SeriesName", "Url" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Media",
                table: "Media");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Media",
                newName: "Key");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Media",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Media",
                table: "Media",
                columns: new[] { "LibraryName", "SeriesName", "Location" });
        }
    }
}
