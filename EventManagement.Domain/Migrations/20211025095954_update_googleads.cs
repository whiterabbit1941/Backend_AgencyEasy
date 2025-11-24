using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class update_googleads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UrlOrName",
                table: "CampaignGoogleAdss",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "CampaignGoogleAdss",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "CampaignGoogleAdss");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CampaignGoogleAdss",
                newName: "UrlOrName");
        }
    }
}
