using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    /// <inheritdoc />
    public partial class LocationsKeyChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Series_LibraryName_SerieName",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Series_LibraryName_SerieName",
                table: "Medias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "SerieName",
                table: "Medias",
                newName: "SeriesName");

            migrationBuilder.RenameColumn(
                name: "SerieName",
                table: "Locations",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "FetchCount",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeriesName",
                table: "Locations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                columns: new[] { "LibraryName", "SeriesName", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Series_LibraryName_SeriesName",
                table: "Locations",
                columns: new[] { "LibraryName", "SeriesName" },
                principalTable: "Series",
                principalColumns: new[] { "LibraryName", "Name" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Series_LibraryName_SeriesName",
                table: "Medias",
                columns: new[] { "LibraryName", "SeriesName" },
                principalTable: "Series",
                principalColumns: new[] { "LibraryName", "Name" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Series_LibraryName_SeriesName",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Series_LibraryName_SeriesName",
                table: "Medias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SeriesName",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "SeriesName",
                table: "Medias",
                newName: "SerieName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Locations",
                newName: "SerieName");

            migrationBuilder.AlterColumn<int>(
                name: "FetchCount",
                table: "Locations",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                columns: new[] { "LibraryName", "SerieName", "Url" });

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Series_LibraryName_SerieName",
                table: "Locations",
                columns: new[] { "LibraryName", "SerieName" },
                principalTable: "Series",
                principalColumns: new[] { "LibraryName", "Name" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Series_LibraryName_SerieName",
                table: "Medias",
                columns: new[] { "LibraryName", "SerieName" },
                principalTable: "Series",
                principalColumns: new[] { "LibraryName", "Name" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
