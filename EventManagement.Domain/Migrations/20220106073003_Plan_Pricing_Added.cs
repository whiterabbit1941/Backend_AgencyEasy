using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class Plan_Pricing_Added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefaultPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxProjects = table.Column<int>(type: "int", nullable: false),
                    MaxTeamUsers = table.Column<int>(type: "int", nullable: false),
                    MaxClientUsers = table.Column<int>(type: "int", nullable: false),
                    MaxKeywordsPerProject = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descriptions = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 250, nullable: false),
                    DefaultPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiredOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentProfileId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    MaxProjects = table.Column<int>(type: "int", nullable: false),
                    MaxTeamUsers = table.Column<int>(type: "int", nullable: false),
                    MaxClientUsers = table.Column<int>(type: "int", nullable: false),
                    MaxKeywordsPerProject = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyPlans_Companys_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyPlans_DefaultPlans_DefaultPlanId",
                        column: x => x.DefaultPlanId,
                        principalTable: "DefaultPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeatureID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Visibility = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanDetails_DefaultPlans_DefaultPlanId",
                        column: x => x.DefaultPlanId,
                        principalTable: "DefaultPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanDetails_Features_FeatureID",
                        column: x => x.FeatureID,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DefaultPlans",
                columns: new[] { "Id", "Cost", "CreatedBy", "CreatedOn", "MaxClientUsers", "MaxKeywordsPerProject", "MaxProjects", "MaxTeamUsers", "Name", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("ba57022f-eb81-4eb5-b590-ef6d418b1db9"), 0m, "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 0, 0, 0, "FREE", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), 29m, "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 50, 3, 0, "STARTUP", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), 79m, "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 100, 10, 0, "AGENCY", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), 86.9m, "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 100, 11, 0, "CUSTOM", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "Descriptions", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "WhiteLableClientDashboard", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "WeeklyRankTracing", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "GoogleAnalyticsIntegration", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "GoogleSearchConsoleIntegration", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("f558f102-2408-4a80-b7a9-e195adefba55"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "GoogleAdsIntegration", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "FacebookAnalyticsIntegration", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "LinkedInIntegration", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "InstagramIntegration", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "FacebookAdsIntegration", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "CustomBrandedReportCreator", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "AutomatedReporting", "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "PlanDetails",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DefaultPlanId", "FeatureID", "UpdatedBy", "UpdatedOn", "Visibility" },
                values: new object[,]
                {
                    { new Guid("a2417fde-fe5a-4843-9f44-8679acb8071e"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("9c6f9a3a-acb7-4e6c-97b9-4b4cd7af057b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("cdf22bd6-658c-4cf0-8d46-a5b9b9b39c9e"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("cb5530f3-2534-4b64-b6e2-8616ea013bc4"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("b865b74a-4d67-41c2-8f64-6465547c2193"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("5a66f69d-352b-4d6e-9e08-83e421785456"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("ac3adbb3-7264-4b28-87e4-9dc34f7ea4c6"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("30cf91b2-298f-4914-a389-478a6ccdbf0b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("f13e1e86-a095-4392-9106-a78c8bcba87e"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("62c6d75b-b852-4016-a0ab-aaaab522e921"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("6f6c4c83-428c-423c-8229-b78ca2e2b6c7"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("34091aff-4bf0-419b-9975-98121ee34c20"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("47d1580e-34c8-43ad-a161-6cd089a5e367"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("3d361b1e-61e7-4859-9b71-f68c651fdefa"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("13bbcd8b-4c4e-4fbe-a633-be877f2a7af6"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("88997ec9-05cf-4786-8701-7196bd66f692"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("eb513d35-cd9c-4858-b2b7-c23791f05248"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("2ea0b2eb-b1ea-470e-a6bb-8ebf3b7f023b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("f558f102-2408-4a80-b7a9-e195adefba55"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("c82f65ec-c284-4bbd-9ec1-9ae728554e05"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("f558f102-2408-4a80-b7a9-e195adefba55"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("ee432552-d73b-4f14-b2e4-6e4d60762e4e"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("f558f102-2408-4a80-b7a9-e195adefba55"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("fb37a693-0fec-40c2-b77b-738407a38eb4"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("f2424157-ddfd-4642-95d1-d0f45c988ddd"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("f597de30-e986-4bf8-a4a7-5cee04c99a74"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("c488bded-f58e-4896-9e65-add7ded51fd6"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("9a8f1cba-5143-4ef8-b83c-eb676fd35b7d"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("e1ac379b-c116-41a5-8614-c01adcb484d3"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("7659538c-fb4f-4d81-a013-3905dfc9754b"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("3e41c640-745f-420b-bfad-0f6d54d45212"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("3b19feae-4a59-4249-8f74-09916864d9f2"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("87941bfd-58e0-4a6b-a514-076174350d23"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("a7242d9a-c43f-4c53-8e21-a5b2374574d6"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"), new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { new Guid("8a1ad249-96fb-488b-b747-4418f0064e86"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2021, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyPlans_CompanyId",
                table: "CompanyPlans",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyPlans_DefaultPlanId",
                table: "CompanyPlans",
                column: "DefaultPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDetails_DefaultPlanId",
                table: "PlanDetails",
                column: "DefaultPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanDetails_FeatureID",
                table: "PlanDetails",
                column: "FeatureID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyPlans");

            migrationBuilder.DropTable(
                name: "PlanDetails");

            migrationBuilder.DropTable(
                name: "DefaultPlans");

            migrationBuilder.DropTable(
                name: "Features");
        }
    }
}
