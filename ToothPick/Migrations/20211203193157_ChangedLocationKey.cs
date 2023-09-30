using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    public partial class ChangedLocationKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.AlterColumn<string>(
                name: "MediaXPath",
                table: "Locations",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyRegex",
                table: "Locations",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                columns: new[] { "LibraryName", "SerieName", "Url", "MediaXPath", "KeyRegex" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-2-youtube.com" },
                column: "Value",
                value: "youtube\\.com\\/watch\\?v=(.*)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.AlterColumn<string>(
                name: "KeyRegex",
                table: "Locations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "MediaXPath",
                table: "Locations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                columns: new[] { "LibraryName", "SerieName", "Url" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumns: new[] { "Environment", "Name" },
                keyValues: new object[] { "Default", "DefaultKeyRegex-2-youtube.com" },
                column: "Value",
                value: "youtube\\.com\\/watch\\?v=(.*)&list");
        }
    }
}
