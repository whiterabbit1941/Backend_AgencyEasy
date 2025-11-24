using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class ChangeSpellingOfCampaignIDInFacebookTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignFacebooks_Campaigns_CmapaignID",
                table: "CampaignFacebooks");

            migrationBuilder.DropIndex(
                name: "IX_CampaignFacebooks_CmapaignID",
                table: "CampaignFacebooks");

            migrationBuilder.DropColumn(
                name: "CmapaignID",
                table: "CampaignFacebooks");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignFacebooks_CampaignID",
                table: "CampaignFacebooks",
                column: "CampaignID");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignFacebooks_Campaigns_CampaignID",
                table: "CampaignFacebooks",
                column: "CampaignID",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignFacebooks_Campaigns_CampaignID",
                table: "CampaignFacebooks");

            migrationBuilder.DropIndex(
                name: "IX_CampaignFacebooks_CampaignID",
                table: "CampaignFacebooks");

            migrationBuilder.AddColumn<Guid>(
                name: "CmapaignID",
                table: "CampaignFacebooks",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CampaignFacebooks_CmapaignID",
                table: "CampaignFacebooks",
                column: "CmapaignID");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignFacebooks_Campaigns_CmapaignID",
                table: "CampaignFacebooks",
                column: "CmapaignID",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
