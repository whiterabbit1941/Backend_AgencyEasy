using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class CreateTableRankingGraph : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "StripePayments",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RankingGraphs",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    AvragePosition = table.Column<int>(nullable: false),
                    Month = table.Column<string>(nullable: true),
                    CampaignId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingGraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankingGraphs_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StripePayments_CampaignId",
                table: "StripePayments",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_StripePayments_PlanId",
                table: "StripePayments",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_StripePayments_UserId",
                table: "StripePayments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingGraphs_CampaignId",
                table: "RankingGraphs",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_StripePayments_Campaigns_CampaignId",
                table: "StripePayments",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StripePayments_Plans_PlanId",
                table: "StripePayments",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StripePayments_AspNetUsers_UserId",
                table: "StripePayments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StripePayments_Campaigns_CampaignId",
                table: "StripePayments");

            migrationBuilder.DropForeignKey(
                name: "FK_StripePayments_Plans_PlanId",
                table: "StripePayments");

            migrationBuilder.DropForeignKey(
                name: "FK_StripePayments_AspNetUsers_UserId",
                table: "StripePayments");

            migrationBuilder.DropTable(
                name: "RankingGraphs");

            migrationBuilder.DropIndex(
                name: "IX_StripePayments_CampaignId",
                table: "StripePayments");

            migrationBuilder.DropIndex(
                name: "IX_StripePayments_PlanId",
                table: "StripePayments");

            migrationBuilder.DropIndex(
                name: "IX_StripePayments_UserId",
                table: "StripePayments");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "StripePayments",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
