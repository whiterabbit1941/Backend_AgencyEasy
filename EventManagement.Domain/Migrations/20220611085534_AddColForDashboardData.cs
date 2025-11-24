using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddColForDashboardData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Conversions",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionDashboardLambda",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gsc",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateDashboardDate",
                table: "Campaigns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Ranking",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Traffic",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrafficGa4",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Conversions",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "ExceptionDashboardLambda",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Gsc",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "LastUpdateDashboardDate",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Ranking",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Traffic",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TrafficGa4",
                table: "Campaigns");
        }
    }
}
