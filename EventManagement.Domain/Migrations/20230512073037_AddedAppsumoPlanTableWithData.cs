using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddedAppsumoPlanTableWithData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppsumoPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    AppsumoPlanId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxProjects = table.Column<int>(type: "int", nullable: false),
                    MaxKeywordsPerProject = table.Column<int>(type: "int", nullable: false),
                    WhitelabelSupport = table.Column<bool>(type: "bit", nullable: false),
                    MaxTeamUsers = table.Column<int>(type: "int", nullable: false),
                    MaxClientUsers = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppsumoPlans", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppsumoPlans",
                columns: new[] { "Id", "AppsumoPlanId", "Cost", "CreatedBy", "CreatedOn", "MaxClientUsers", "MaxKeywordsPerProject", "MaxProjects", "MaxTeamUsers", "Name", "UpdatedBy", "UpdatedOn", "WhitelabelSupport" },
                values: new object[,]
                {
                    { new Guid("934b6e66-337e-4944-9c6a-788a194f3f0b"), "agencyeasy_tier1", 59m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 50, 3, 0, "License Tier 1", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false },
                    { new Guid("9f91c98c-207c-41e6-bb44-1ac37d8c6e53"), "agencyeasy_tier2", 119m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 100, 6, 0, "License Tier 2", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("a2b83140-f803-45ca-9209-35fc1888175f"), "agencyeasy_tier3", 179m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 150, 12, 0, "License Tier 3", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("1f5b1d53-8619-494d-821d-fa98ac9d8c11"), "agencyeasy_tier4", 239m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 200, 24, 0, "License Tier 4", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("b6ddf793-e87f-4b2d-9822-92b8226d8301"), "agencyeasy_tier5", 299m, "Migration", new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 0, 100, 0, "License Tier 5", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppsumoPlans");
        }
    }
}
