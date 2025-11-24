using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddPlanInDefaultPlans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DefaultPlans",
                columns: new[] { "Id", "Cost", "CreatedBy", "CreatedOn", "MaxClientUsers", "MaxKeywordsPerProject", "MaxProjects", "MaxTeamUsers", "Name", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), 49m, "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 100, 10, 0, "LIFETIMEDEAL", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                });


            migrationBuilder.InsertData(
               table: "PlanDetails",
               columns: new[] { "Id", "CreatedBy", "CreatedOn", "DefaultPlanId", "FeatureID", "UpdatedBy", "UpdatedOn", "Visibility" },
               values: new object[,]
               {            
                    { new Guid("f5cbc0f9-a1e6-48e8-a8d7-1327b9c7464b"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },                   
                    { new Guid("c2f5640d-fc7f-4bcb-9a8a-b8eb143069ab"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },                    
                    { new Guid("ba84b30c-5722-4096-bb6f-19c524fb33d8"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("e778f499-3d78-46db-aca2-e83fdec0faeb"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("e01a03a9-9e84-418e-8249-ed04e48a434d"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },                                        
                    { new Guid("3cf6ce46-4e41-46ff-bcac-8f84af86f0e3"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },                   
                    { new Guid("74f56d34-2973-4cc4-8ee3-a7813be5c515"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("f558f102-2408-4a80-b7a9-e195adefba55"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },                                       
                    { new Guid("ad3f0179-27df-4390-a140-2e2956bff8ac"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },                                        
                    { new Guid("ca0e0e6b-1e2e-41e1-b6c4-8b8a47d3da6c"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },                                        
                    { new Guid("77691102-1944-46da-842c-d4cd7c0490f4"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("052e9928-143f-4eba-84fe-57c9fb1244f0"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3f63036-87a2-4756-aa77-ed51b6d717eb"), new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"), "Migration", new DateTime(2022, 06, 06, 0, 0, 0, 0, DateTimeKind.Unspecified), true },                                       
               });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM PlanDetails WHERE DefaultPlanId='f3f63036-87a2-4756-aa77-ed51b6d717eb'", false);
            migrationBuilder.Sql("DELETE FROM DefaultPlans WHERE Id='f3f63036-87a2-4756-aa77-ed51b6d717eb'", false);           
            
        }
    }
}
