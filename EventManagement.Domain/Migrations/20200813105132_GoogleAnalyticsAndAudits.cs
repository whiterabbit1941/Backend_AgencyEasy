using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class GoogleAnalyticsAndAudits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auditss",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    WebsiteUrl = table.Column<string>(nullable: true),
                    Grade = table.Column<string>(maxLength: 10, nullable: true),
                    IsSent = table.Column<bool>(nullable: false),
                    TaskId = table.Column<long>(nullable: false),
                    Status = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditss", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoogleAccountSetups",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    AccessToken = table.Column<string>(nullable: true),
                    RefreshToken = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(maxLength: 250, nullable: true),
                    UserName = table.Column<string>(maxLength: 250, nullable: true),
                    IsAuthorize = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleAccountSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoogleAnalyticsAccounts",
                columns: table => new
                {
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    CampaignID = table.Column<string>(nullable: true),
                    GoogleAccountSetupID = table.Column<Guid>(nullable: false),
                    AccountID = table.Column<string>(maxLength: 250, nullable: true),
                    AccountName = table.Column<string>(maxLength: 250, nullable: true),
                    WebsiteUrl = table.Column<string>(nullable: true),
                    PropertyID = table.Column<string>(maxLength: 250, nullable: true),
                    ViewName = table.Column<string>(maxLength: 250, nullable: true),
                    ViewID = table.Column<string>(maxLength: 250, nullable: true),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleAnalyticsAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoogleAnalyticsAccounts_GoogleAccountSetups_GoogleAccountSetupID",
                        column: x => x.GoogleAccountSetupID,
                        principalTable: "GoogleAccountSetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleAnalyticsAccounts_GoogleAccountSetupID",
                table: "GoogleAnalyticsAccounts",
                column: "GoogleAccountSetupID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditss");

            migrationBuilder.DropTable(
                name: "GoogleAnalyticsAccounts");

            migrationBuilder.DropTable(
                name: "GoogleAccountSetups");
        }
    }
}
