using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace EventManagement.Domain.Migrations
{
    public partial class Added89DefaultPlans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
               table: "DefaultPlans",
               columns: new[] { "Id", "Cost", "CreatedBy", "CreatedOn", "MaxClientUsers", "MaxKeywordsPerProject", "MaxProjects", "MaxTeamUsers", "Name", "UpdatedBy", "UpdatedOn", "IsVisible" },
               values: new object[,]
               {
                    { new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), 89m, "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 50, 6, 0, "ONETIMEDEAL", null, new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true},
               });


            migrationBuilder.InsertData(
               table: "PlanDetails",
               columns: new[] { "Id", "CreatedBy", "CreatedOn", "DefaultPlanId", "FeatureID", "UpdatedBy", "UpdatedOn", "Visibility" },
               values: new object[,]
               {
                    { new Guid("42ce072d-e27d-405e-8ebd-40bab2f4f18d"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("28d11cbd-5043-4aa5-8318-f1e81b550e49"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("23d090a0-f4d0-4109-9b0b-51eb2a9966ba"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("d3aebf11-24ae-4b64-8ce9-a876884bee74"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("a0f22e1c-5825-4920-9fd7-4d4b98172bd4"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("817a6c20-6815-4bd9-b760-47274613351e"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("cdba1911-a5ba-4c47-844f-52c859d2b7ca"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("f558f102-2408-4a80-b7a9-e195adefba55"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("7fe7e658-143e-4e8e-b913-99c3aa524851"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("db6c4572-bf06-41d3-8ef7-982f723b5634"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("314ef161-0184-4354-b913-5746eff4e530"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("a28d1e2f-ee50-4c00-b6d9-11a00a01c3f3"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("15640ed0-b078-4547-89b7-e2520617cfb3"), new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"), "Migration", new DateTime(2022, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
               });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM PlanDetails WHERE DefaultPlanId='15640ed0-b078-4547-89b7-e2520617cfb3'", false);
            migrationBuilder.Sql("DELETE FROM DefaultPlans WHERE Id='15640ed0-b078-4547-89b7-e2520617cfb3'", false);
        }
    }
}
