using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class DomainComplexId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ShopDomains",
                table: "ShopDomains");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShopDomains",
                table: "ShopDomains",
                columns: new[] { "Domain", "ShopId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ShopDomains",
                table: "ShopDomains");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShopDomains",
                table: "ShopDomains",
                column: "Domain");
        }
    }
}
