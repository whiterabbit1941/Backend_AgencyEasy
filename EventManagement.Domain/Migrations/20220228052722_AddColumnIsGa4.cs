using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddColumnIsGa4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGa4",
                table: "CampaignGoogleAnalyticss",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGa4",
                table: "CampaignGoogleAnalyticss");
        }
    }
}
