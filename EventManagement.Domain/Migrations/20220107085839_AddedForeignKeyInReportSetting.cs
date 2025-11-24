using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddedForeignKeyInReportSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ReportSettings_CampaignId",
                table: "ReportSettings",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSettings_CompanyId",
                table: "ReportSettings",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportSettings_Campaigns_CampaignId",
                table: "ReportSettings",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportSettings_Campaigns_CampaignId",
                table: "ReportSettings");

            migrationBuilder.DropIndex(
                name: "IX_ReportSettings_CampaignId",
                table: "ReportSettings");

            migrationBuilder.DropIndex(
                name: "IX_ReportSettings_CompanyId",
                table: "ReportSettings");
        }
    }
}
