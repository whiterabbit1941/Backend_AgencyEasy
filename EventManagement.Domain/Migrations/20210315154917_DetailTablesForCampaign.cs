using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class DetailTablesForCampaign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignFacebooks",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    UrlOrName = table.Column<string>(maxLength: 250, nullable: false),
                    CmapaignID = table.Column<Guid>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignFacebooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignFacebooks_Campaigns_CmapaignID",
                        column: x => x.CmapaignID,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignGoogleAdss",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    UrlOrName = table.Column<string>(maxLength: 250, nullable: false),
                    CmapaignID = table.Column<Guid>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignGoogleAdss", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignGoogleAdss_Campaigns_CmapaignID",
                        column: x => x.CmapaignID,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignGoogleAnalyticss",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    UrlOrName = table.Column<string>(maxLength: 250, nullable: false),
                    CmapaignID = table.Column<Guid>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignGoogleAnalyticss", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignGoogleAnalyticss_Campaigns_CmapaignID",
                        column: x => x.CmapaignID,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignGSCs",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    UrlOrName = table.Column<string>(maxLength: 250, nullable: false),
                    CmapaignID = table.Column<Guid>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignGSCs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignGSCs_Campaigns_CmapaignID",
                        column: x => x.CmapaignID,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignFacebooks_CmapaignID",
                table: "CampaignFacebooks",
                column: "CmapaignID");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignGoogleAdss_CmapaignID",
                table: "CampaignGoogleAdss",
                column: "CmapaignID");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignGoogleAnalyticss_CmapaignID",
                table: "CampaignGoogleAnalyticss",
                column: "CmapaignID");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignGSCs_CmapaignID",
                table: "CampaignGSCs",
                column: "CmapaignID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignFacebooks");

            migrationBuilder.DropTable(
                name: "CampaignGoogleAdss");

            migrationBuilder.DropTable(
                name: "CampaignGoogleAnalyticss");

            migrationBuilder.DropTable(
                name: "CampaignGSCs");
        }
    }
}
