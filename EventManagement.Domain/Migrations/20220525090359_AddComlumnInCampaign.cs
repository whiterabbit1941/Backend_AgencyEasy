using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddComlumnInCampaign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LambdaLogger",
                table: "Serps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateSerpDate",
                table: "Campaigns",
                type: "datetime2",
                nullable: false,
                defaultValue: "2022-05-18 12:20:51.3985853");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LambdaLogger",
                table: "Serps");

            migrationBuilder.DropColumn(
                name: "LastUpdateSerpDate",
                table: "Campaigns");
        }
    }
}
