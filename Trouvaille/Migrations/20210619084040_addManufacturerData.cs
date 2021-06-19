using Microsoft.EntityFrameworkCore.Migrations;

namespace Trouvaille3.Migrations
{
    public partial class addManufacturerData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CatalogId",
                table: "Manufacturer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Manufacturer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CatalogId",
                table: "Manufacturer");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Manufacturer");
        }
    }
}
