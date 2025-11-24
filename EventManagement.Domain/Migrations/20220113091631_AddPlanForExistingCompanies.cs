using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddPlanForExistingCompanies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "DefaultPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"),
                column: "IsVisible",
                value: true);

            migrationBuilder.UpdateData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"),
                column: "IsVisible",
                value: true);

            migrationBuilder.UpdateData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"),
                column: "IsVisible",
                value: true);

            migrationBuilder.UpdateData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("ba57022f-eb81-4eb5-b590-ef6d418b1db9"),
                column: "IsVisible",
                value: true);

            migrationBuilder.InsertData(
                table: "DefaultPlans",
                columns: new[] { "Id", "Cost", "CreatedBy", "CreatedOn", "IsVisible", "MaxClientUsers", "MaxKeywordsPerProject", "MaxProjects", "MaxTeamUsers", "Name", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), 0m, "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 989, DateTimeKind.Local).AddTicks(169), false, 0, 0, 0, 0, "CUSTOM FREE", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "CompanyPlans",
                columns: new[] { "Id", "Active", "CompanyId", "CreatedBy", "CreatedOn", "DefaultPlanId", "ExpiredOn", "MaxClientUsers", "MaxKeywordsPerProject", "MaxProjects", "MaxTeamUsers", "PaymentProfileId", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("6d5f4ea9-7d37-41dc-985b-054c929d9761"), true, new Guid("5f5fe5c0-c10a-4dde-bb82-08d95afbd55c"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 990, DateTimeKind.Local).AddTicks(9709), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 990, DateTimeKind.Local).AddTicks(6293), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 990, DateTimeKind.Local).AddTicks(9719) },
                    { new Guid("d0904499-8110-4dbb-9c18-5f17c7c51acc"), true, new Guid("96fb88fc-7c25-4db1-a206-08d9aa8cae87"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(467), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(464), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(468) },
                    { new Guid("f728d187-a094-4564-82ed-c9a89bf98615"), true, new Guid("26f8e007-677a-415e-f76b-08d98c837f57"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(446), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(444), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(447) },
                    { new Guid("66349c8d-df5b-4154-be15-10575417f761"), true, new Guid("26f8e007-677a-415e-f76b-08d98c837f57"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(436), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(434), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(437) },
                    { new Guid("0f1973f5-a38a-4269-8ba8-6cb09a749d0b"), true, new Guid("0f43c3e6-4e78-4301-f76a-08d98c837f57"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(426), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(423), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(427) },
                    { new Guid("6aac525a-fe27-445a-b87d-ecd561cff29b"), true, new Guid("29689f6e-e003-4dbf-c131-08d97e7dd203"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(416), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(413), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(417) },
                    { new Guid("eb4044c5-4f45-4b7f-b5cc-af35abf6eac8"), true, new Guid("1703c418-74c0-4bb2-476f-08d979207a67"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(405), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(403), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(406) },
                    { new Guid("b0fd86bc-a818-4b06-a597-ddeabe3c820f"), true, new Guid("479f00df-dbb7-47e5-a205-08d9aa8cae87"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(457), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(455), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(458) },
                    { new Guid("d477ffa6-1404-40c0-b6f2-a5b0dd0e3c99"), true, new Guid("081011a2-cb77-4b16-476d-08d979207a67"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(383), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(380), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(384) },
                    { new Guid("01d42519-ab4a-46e0-9bdb-b0e979df2a5c"), true, new Guid("9cae5feb-f0be-4fe9-476c-08d979207a67"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(372), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(370), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(373) },
                    { new Guid("0b429d02-8402-4bbd-aa6b-6cb5a4da1e6b"), true, new Guid("4587a7e9-b546-40b2-476b-08d979207a67"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(362), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(359), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(363) },
                    { new Guid("f1bc45d4-1bb0-425f-9aec-b3cc522fa26c"), true, new Guid("01041fcf-182f-41fd-476a-08d979207a67"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(350), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(347), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(352) },
                    { new Guid("e0b4f46e-871d-448f-ad62-24d50bfd7c49"), true, new Guid("ddd31200-0a7a-4719-d49b-08d96c484d6b"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(325), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(233), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(327) },
                    { new Guid("e3564bd5-b6f2-423c-aebd-d594aa964112"), true, new Guid("7d4b196c-4564-4a79-476e-08d979207a67"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(393), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new DateTime(2023, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(391), 0, 0, 0, 0, "customfree", "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(395) }
                });

            migrationBuilder.InsertData(
                table: "PlanDetails",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DefaultPlanId", "FeatureID", "UpdatedBy", "UpdatedOn", "Visibility" },
                values: new object[,]
                {
                    { new Guid("49c9b4f4-fcd2-4472-9d9c-f18053ad0e79"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5501), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5503), true },
                    { new Guid("2c496c99-796e-4906-a283-1b400e82babb"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5493), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("f558f102-2408-4a80-b7a9-e195adefba55"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5494), true },
                    { new Guid("b01ca26a-dbbb-43a2-a7d5-ab60760ef350"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5484), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5486), true },
                    { new Guid("5e339f81-e5af-4382-a03e-fbbf558f4114"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5475), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5477), true },
                    { new Guid("f60045ec-467e-41db-8ad6-8a99a374cab7"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5451), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5453), true },
                    { new Guid("48ceaedb-4c56-4976-8873-f34f3046418e"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5460), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5461), true },
                    { new Guid("a1e61d02-d1b3-4697-be38-aaf830a8eae2"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5443), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5444), true },
                    { new Guid("3896a69e-ac9a-4574-b21d-72a1c6011152"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5428), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5436), true },
                    { new Guid("e1a4d829-a85a-44e3-a0ca-05e66b9cbcfd"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5510), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5511), true },
                    { new Guid("9e6fb990-13b3-4945-b863-c936af8ff209"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5467), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5469), true },
                    { new Guid("85ad5833-5d35-48a2-a313-f7c4d3388938"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5517), new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"), new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"), "Migration", new DateTime(2022, 1, 13, 14, 46, 29, 991, DateTimeKind.Local).AddTicks(5519), true }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("01d42519-ab4a-46e0-9bdb-b0e979df2a5c"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("0b429d02-8402-4bbd-aa6b-6cb5a4da1e6b"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("0f1973f5-a38a-4269-8ba8-6cb09a749d0b"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("66349c8d-df5b-4154-be15-10575417f761"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("6aac525a-fe27-445a-b87d-ecd561cff29b"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("6d5f4ea9-7d37-41dc-985b-054c929d9761"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("b0fd86bc-a818-4b06-a597-ddeabe3c820f"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("d0904499-8110-4dbb-9c18-5f17c7c51acc"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("d477ffa6-1404-40c0-b6f2-a5b0dd0e3c99"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("e0b4f46e-871d-448f-ad62-24d50bfd7c49"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("e3564bd5-b6f2-423c-aebd-d594aa964112"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("eb4044c5-4f45-4b7f-b5cc-af35abf6eac8"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("f1bc45d4-1bb0-425f-9aec-b3cc522fa26c"));

            migrationBuilder.DeleteData(
                table: "CompanyPlans",
                keyColumn: "Id",
                keyValue: new Guid("f728d187-a094-4564-82ed-c9a89bf98615"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("2c496c99-796e-4906-a283-1b400e82babb"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("3896a69e-ac9a-4574-b21d-72a1c6011152"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("48ceaedb-4c56-4976-8873-f34f3046418e"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("49c9b4f4-fcd2-4472-9d9c-f18053ad0e79"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("5e339f81-e5af-4382-a03e-fbbf558f4114"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("85ad5833-5d35-48a2-a313-f7c4d3388938"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("9e6fb990-13b3-4945-b863-c936af8ff209"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("a1e61d02-d1b3-4697-be38-aaf830a8eae2"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("b01ca26a-dbbb-43a2-a7d5-ab60760ef350"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("e1a4d829-a85a-44e3-a0ca-05e66b9cbcfd"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("f60045ec-467e-41db-8ad6-8a99a374cab7"));

            migrationBuilder.DeleteData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"));
 
            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "DefaultPlans");
        }
    }
}
