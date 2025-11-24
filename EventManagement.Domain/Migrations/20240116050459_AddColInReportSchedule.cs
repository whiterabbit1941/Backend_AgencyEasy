using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddColInReportSchedule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HtmlFooter",
                table: "ReportSchedulings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "<p>Regards,</p>"
               );

            migrationBuilder.AddColumn<string>(
                name: "HtmlHeader",
                table: "ReportSchedulings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "<p>Hello,</p><p>Greetings.</p><p>Please find attached, along with this email.</p><p>This is an automated email.</p>");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HtmlFooter",
                table: "ReportSchedulings");

            migrationBuilder.DropColumn(
                name: "HtmlHeader",
                table: "ReportSchedulings");
        }
    }
}
