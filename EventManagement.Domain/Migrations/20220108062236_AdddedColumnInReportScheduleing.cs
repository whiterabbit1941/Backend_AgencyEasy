using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AdddedColumnInReportScheduleing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "ReportSchedulings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Scheduled",
                table: "ReportSchedulings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "ReportSchedulings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "ReportSchedulings");

            migrationBuilder.DropColumn(
                name: "Scheduled",
                table: "ReportSchedulings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ReportSchedulings");
        }
    }
}
