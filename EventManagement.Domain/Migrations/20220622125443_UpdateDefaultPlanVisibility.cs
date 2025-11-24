using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace EventManagement.Domain.Migrations
{
    public partial class UpdateDefaultPlanVisibility : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
               table: "DefaultPlans",
               keyColumn: "Id",
               keyValue: new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"),
               columns: new[] { "CreatedOn", "IsVisible", "UpdatedOn" },
               values: new object[] { new DateTime(2022, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), true, new DateTime(2022, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM DefaultPlans WHERE Id='f3f63036-87a2-4756-aa77-ed51b6d717eb'", false);
        }
    }
}
