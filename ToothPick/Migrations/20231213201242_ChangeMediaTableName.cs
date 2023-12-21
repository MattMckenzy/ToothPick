using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMediaTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Libraries_LibraryName",
                table: "Medias");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Series_LibraryName_SeriesName",
                table: "Medias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Medias",
                table: "Medias");

            migrationBuilder.RenameTable(
                name: "Medias",
                newName: "Media");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Media",
                table: "Media",
                columns: new[] { "LibraryName", "SeriesName", "Location" });

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Libraries_LibraryName",
                table: "Media",
                column: "LibraryName",
                principalTable: "Libraries",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Series_LibraryName_SeriesName",
                table: "Media",
                columns: new[] { "LibraryName", "SeriesName" },
                principalTable: "Series",
                principalColumns: new[] { "LibraryName", "Name" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_Libraries_LibraryName",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_Series_LibraryName_SeriesName",
                table: "Media");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Media",
                table: "Media");

            migrationBuilder.RenameTable(
                name: "Media",
                newName: "Medias");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Medias",
                table: "Medias",
                columns: new[] { "LibraryName", "SeriesName", "Location" });

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Libraries_LibraryName",
                table: "Medias",
                column: "LibraryName",
                principalTable: "Libraries",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Series_LibraryName_SeriesName",
                table: "Medias",
                columns: new[] { "LibraryName", "SeriesName" },
                principalTable: "Series",
                principalColumns: new[] { "LibraryName", "Name" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
