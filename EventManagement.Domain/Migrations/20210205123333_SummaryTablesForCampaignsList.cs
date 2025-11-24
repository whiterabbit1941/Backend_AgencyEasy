using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class SummaryTablesForCampaignsList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "RankingGraphs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GoogleAdsSummarys",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    AvragePosition = table.Column<int>(nullable: false),
                    Month = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    CampaignId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleAdsSummarys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoogleAdsSummarys_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GscSummarys",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    AvragePosition = table.Column<int>(nullable: false),
                    Month = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    CampaignId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GscSummarys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GscSummarys_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialMediaSUmmmarys",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    AvragePosition = table.Column<int>(nullable: false),
                    Month = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    CampaignId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialMediaSUmmmarys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialMediaSUmmmarys_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrafficSummarys",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    AvragePosition = table.Column<int>(nullable: false),
                    Month = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    CampaignId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficSummarys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafficSummarys_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleAdsSummarys_CampaignId",
                table: "GoogleAdsSummarys",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_GscSummarys_CampaignId",
                table: "GscSummarys",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialMediaSUmmmarys_CampaignId",
                table: "SocialMediaSUmmmarys",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficSummarys_CampaignId",
                table: "TrafficSummarys",
                column: "CampaignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleAdsSummarys");

            migrationBuilder.DropTable(
                name: "GscSummarys");

            migrationBuilder.DropTable(
                name: "SocialMediaSUmmmarys");

            migrationBuilder.DropTable(
                name: "TrafficSummarys");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "RankingGraphs");
        }
    }
}
