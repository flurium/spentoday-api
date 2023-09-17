using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class StorageFileContainer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserImages");

            migrationBuilder.AddColumn<string>(
                name: "LogoBucket",
                table: "Shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoKey",
                table: "Shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoProvider",
                table: "Shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageBucket",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageKey",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageProvider",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoBucket",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "LogoKey",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "LogoProvider",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ImageBucket",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ImageKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ImageProvider",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "UserImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Bucket = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserImages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserImages_UserId",
                table: "UserImages",
                column: "UserId",
                unique: true);
        }
    }
}