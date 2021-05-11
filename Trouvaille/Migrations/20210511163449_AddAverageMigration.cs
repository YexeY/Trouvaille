using Microsoft.EntityFrameworkCore.Migrations;

namespace Trouvaille3.Migrations
{
    public partial class AddAverageMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Product",
                type: "decimal(16,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Product");
        }
    }
}
