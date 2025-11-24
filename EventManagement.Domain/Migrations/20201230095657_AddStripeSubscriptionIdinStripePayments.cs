using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddStripeSubscriptionIdinStripePayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "StripePayments");

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "StripePayments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "StripePayments");

            migrationBuilder.AddColumn<Guid>(
                name: "CampaignId",
                table: "StripePayments",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
