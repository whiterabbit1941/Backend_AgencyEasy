using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class WooCommerceSetup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignWooCommerces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumerKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsumerSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShopUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignWooCommerces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignWooCommerces_Campaigns_CampaignID",
                        column: x => x.CampaignID,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignWooCommerces_CampaignID",
                table: "CampaignWooCommerces",
                column: "CampaignID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignWooCommerces");
        }
    }
}
