using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToothPick.Migrations
{
    /// <inheritdoc />
    public partial class AddedReverseFetch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReverseFetch",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReverseFetch",
                table: "Locations");
        }
    }
}
