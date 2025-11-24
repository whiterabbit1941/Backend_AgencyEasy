using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace EventManagement.Domain.Migrations
{
    public partial class UpdateFreePlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                        migrationBuilder.UpdateData(
              table: "DefaultPlans",
              keyColumn: "Id",
              keyValue: new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"),
              columns: new[] { "MaxProjects", "MaxKeywordsPerProject" },
              values: new object[] { "3", "100" });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

                        migrationBuilder.UpdateData(
              table: "DefaultPlans",
              keyColumn: "Id",
              keyValue: new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"),
              columns: new[] { "MaxProjects", "MaxKeywordsPerProject" },
              values: new object[] { "0", "0" });

        }
    }
}
