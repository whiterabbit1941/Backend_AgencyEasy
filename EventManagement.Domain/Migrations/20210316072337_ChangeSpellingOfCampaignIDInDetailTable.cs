using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class ChangeSpellingOfCampaignIDInDetailTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignGoogleAdss_Campaigns_CmapaignID",
                table: "CampaignGoogleAdss");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignGoogleAnalyticss_Campaigns_CmapaignID",
                table: "CampaignGoogleAnalyticss");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignGSCs_Campaigns_CmapaignID",
                table: "CampaignGSCs");

            migrationBuilder.RenameColumn(
                name: "CmapaignID",
                table: "CampaignGSCs",
                newName: "CampaignID");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignGSCs_CmapaignID",
                table: "CampaignGSCs",
                newName: "IX_CampaignGSCs_CampaignID");

            migrationBuilder.RenameColumn(
                name: "CmapaignID",
                table: "CampaignGoogleAnalyticss",
                newName: "CampaignID");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignGoogleAnalyticss_CmapaignID",
                table: "CampaignGoogleAnalyticss",
                newName: "IX_CampaignGoogleAnalyticss_CampaignID");

            migrationBuilder.RenameColumn(
                name: "CmapaignID",
                table: "CampaignGoogleAdss",
                newName: "CampaignID");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignGoogleAdss_CmapaignID",
                table: "CampaignGoogleAdss",
                newName: "IX_CampaignGoogleAdss_CampaignID");

            migrationBuilder.AddColumn<Guid>(
                name: "CampaignID",
                table: "CampaignFacebooks",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignGoogleAdss_Campaigns_CampaignID",
                table: "CampaignGoogleAdss",
                column: "CampaignID",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignGoogleAnalyticss_Campaigns_CampaignID",
                table: "CampaignGoogleAnalyticss",
                column: "CampaignID",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignGSCs_Campaigns_CampaignID",
                table: "CampaignGSCs",
                column: "CampaignID",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignGoogleAdss_Campaigns_CampaignID",
                table: "CampaignGoogleAdss");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignGoogleAnalyticss_Campaigns_CampaignID",
                table: "CampaignGoogleAnalyticss");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignGSCs_Campaigns_CampaignID",
                table: "CampaignGSCs");

            migrationBuilder.DropColumn(
                name: "CampaignID",
                table: "CampaignFacebooks");

            migrationBuilder.RenameColumn(
                name: "CampaignID",
                table: "CampaignGSCs",
                newName: "CmapaignID");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignGSCs_CampaignID",
                table: "CampaignGSCs",
                newName: "IX_CampaignGSCs_CmapaignID");

            migrationBuilder.RenameColumn(
                name: "CampaignID",
                table: "CampaignGoogleAnalyticss",
                newName: "CmapaignID");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignGoogleAnalyticss_CampaignID",
                table: "CampaignGoogleAnalyticss",
                newName: "IX_CampaignGoogleAnalyticss_CmapaignID");

            migrationBuilder.RenameColumn(
                name: "CampaignID",
                table: "CampaignGoogleAdss",
                newName: "CmapaignID");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignGoogleAdss_CampaignID",
                table: "CampaignGoogleAdss",
                newName: "IX_CampaignGoogleAdss_CmapaignID");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignGoogleAdss_Campaigns_CmapaignID",
                table: "CampaignGoogleAdss",
                column: "CmapaignID",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignGoogleAnalyticss_Campaigns_CmapaignID",
                table: "CampaignGoogleAnalyticss",
                column: "CmapaignID",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignGSCs_Campaigns_CmapaignID",
                table: "CampaignGSCs",
                column: "CmapaignID",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
