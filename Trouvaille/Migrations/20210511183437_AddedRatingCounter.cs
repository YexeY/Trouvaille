using Microsoft.EntityFrameworkCore.Migrations;

namespace Trouvaille3.Migrations
{
    public partial class AddedRatingCounter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RatingCounter",
                table: "Product",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingCounter",
                table: "Product");
        }
    }
}
