using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddTokensColumnsINDetailTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "CampaignGSCs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "CampaignGSCs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "CampaignGoogleAnalyticss",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "CampaignGoogleAnalyticss",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "CampaignGoogleAdss",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "CampaignGoogleAdss",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "CampaignFacebooks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PageToken",
                table: "CampaignFacebooks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "CampaignFacebooks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "CampaignGSCs");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "CampaignGSCs");

            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "CampaignGoogleAnalyticss");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "CampaignGoogleAnalyticss");

            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "CampaignGoogleAdss");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "CampaignGoogleAdss");

            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "CampaignFacebooks");

            migrationBuilder.DropColumn(
                name: "PageToken",
                table: "CampaignFacebooks");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "CampaignFacebooks");
        }
    }
}
