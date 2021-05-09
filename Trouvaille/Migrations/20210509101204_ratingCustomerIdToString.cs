using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Trouvaille3.Migrations
{
    public partial class ratingCustomerIdToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rating_AspNetUsers_CustomerId1",
                table: "Rating");

            migrationBuilder.DropIndex(
                name: "IX_Rating_CustomerId1",
                table: "Rating");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                table: "Rating");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "Rating",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Rating_CustomerId",
                table: "Rating",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_AspNetUsers_CustomerId",
                table: "Rating",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rating_AspNetUsers_CustomerId",
                table: "Rating");

            migrationBuilder.DropIndex(
                name: "IX_Rating_CustomerId",
                table: "Rating");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Rating",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerId1",
                table: "Rating",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rating_CustomerId1",
                table: "Rating",
                column: "CustomerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Rating_AspNetUsers_CustomerId1",
                table: "Rating",
                column: "CustomerId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
