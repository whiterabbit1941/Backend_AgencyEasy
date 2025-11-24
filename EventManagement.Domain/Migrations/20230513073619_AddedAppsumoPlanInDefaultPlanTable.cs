using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddedAppsumoPlanInDefaultPlanTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DefaultPlans",
                columns: new[] { "Id", "Cost", "CreatedBy", "CreatedOn", "IsVisible", "MaxClientUsers", "MaxKeywordsPerProject", "MaxProjects", "MaxTeamUsers", "Name", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("934b6e66-337e-4944-9c6a-788a194f3f0b"), 59m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 0, 50, 3, 0, "agencyeasy_tier1", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("9f91c98c-207c-41e6-bb44-1ac37d8c6e53"), 119m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 0, 100, 6, 0, "agencyeasy_tier2", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("a2b83140-f803-45ca-9209-35fc1888175f"), 179m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 0, 150, 12, 0, "agencyeasy_tier3", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("1f5b1d53-8619-494d-821d-fa98ac9d8c11"), 239m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 0, 200, 24, 0, "agencyeasy_tier4", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("b6ddf793-e87f-4b2d-9822-92b8226d8301"), 299m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 0, 0, 100, 0, "agencyeasy_tier5", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("1f5b1d53-8619-494d-821d-fa98ac9d8c11"));

            migrationBuilder.DeleteData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("934b6e66-337e-4944-9c6a-788a194f3f0b"));

            migrationBuilder.DeleteData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("9f91c98c-207c-41e6-bb44-1ac37d8c6e53"));

            migrationBuilder.DeleteData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("a2b83140-f803-45ca-9209-35fc1888175f"));

            migrationBuilder.DeleteData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("b6ddf793-e87f-4b2d-9822-92b8226d8301"));
        }
    }
}
