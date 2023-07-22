using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class ShopOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_OwnerId",
                table: "Shops",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_AspNetUsers_OwnerId",
                table: "Shops",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shops_AspNetUsers_OwnerId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_OwnerId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Shops");
        }
    }
}