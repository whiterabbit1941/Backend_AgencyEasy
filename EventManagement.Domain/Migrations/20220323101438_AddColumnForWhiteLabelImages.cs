using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddColumnForWhiteLabelImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DashboardLogo",
                table: "Companys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fevicon",
                table: "Companys",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DashboardLogo",
                table: "Companys");

            migrationBuilder.DropColumn(
                name: "Fevicon",
                table: "Companys");
        }
    }
}
