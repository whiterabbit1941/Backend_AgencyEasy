using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class MailchimpIntegration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MailchimpSettings",
                table: "ReportSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CampaignMailchimps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AccountId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampaignID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignMailchimps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignMailchimps_Campaigns_CampaignID",
                        column: x => x.CampaignID,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMailchimps_CampaignID",
                table: "CampaignMailchimps",
                column: "CampaignID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignMailchimps");

            migrationBuilder.DropColumn(
                name: "MailchimpSettings",
                table: "ReportSettings");
        }
    }
}
