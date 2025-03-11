using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Image_Sorter_DotNet.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTagColourFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColourB",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "ColourG",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "ColourR",
                table: "Tags");

            migrationBuilder.AddColumn<string>(
                name: "ColourHex",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColourHex",
                table: "Tags");

            migrationBuilder.AddColumn<int>(
                name: "ColourB",
                table: "Tags",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ColourG",
                table: "Tags",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ColourR",
                table: "Tags",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
