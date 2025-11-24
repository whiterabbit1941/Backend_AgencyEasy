using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class Serp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Serps",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    CampaignID = table.Column<Guid>(nullable: false),
                    Position = table.Column<int>(nullable: false),
                    Searches = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Keywords = table.Column<string>(nullable: true),
                    Tags = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Serps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Serps_Campaigns_CampaignID",
                        column: x => x.CampaignID,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Serps_CampaignID",
                table: "Serps",
                column: "CampaignID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Serps");
        }
    }
}
