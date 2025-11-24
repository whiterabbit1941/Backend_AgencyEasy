using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddColumnEmailId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailId",
                table: "CampaignGSCs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailId",
                table: "CampaignGoogleAnalyticss",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailId",
                table: "CampaignGoogleAdss",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailId",
                table: "CampaignGSCs");

            migrationBuilder.DropColumn(
                name: "EmailId",
                table: "CampaignGoogleAnalyticss");

            migrationBuilder.DropColumn(
                name: "EmailId",
                table: "CampaignGoogleAdss");
        }
    }
}
