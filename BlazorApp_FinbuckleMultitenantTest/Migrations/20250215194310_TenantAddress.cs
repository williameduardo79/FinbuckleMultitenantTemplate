using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp_FinbuckleMultitenantTest.Migrations
{
    /// <inheritdoc />
    public partial class TenantAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserTenants");

            migrationBuilder.AddColumn<string>(
                name: "TenantAddress",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantAddress",
                table: "Tenants");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserTenants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
