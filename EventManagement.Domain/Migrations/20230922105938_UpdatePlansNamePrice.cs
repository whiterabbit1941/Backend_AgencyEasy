using AutoMapper.QueryableExtensions.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace EventManagement.Domain.Migrations
{
    public partial class UpdatePlansNamePrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            #region Update Plan

            migrationBuilder.UpdateData(
               table: "DefaultPlans",
               keyColumn: "Id",
               keyValue: new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"),
               columns: new[] { "Name", "MaxProjects", "MaxKeywordsPerProject" },
               values: new object[] { "Starter", "5", "250" });

            migrationBuilder.UpdateData(
               table: "DefaultPlans",
               keyColumn: "Id",
               keyValue: new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
               columns: new[] { "Name", "MaxProjects", "MaxKeywordsPerProject" },
               values: new object[] { "Growth", "20", "2000" });

            migrationBuilder.InsertData(
                table: "DefaultPlans",
                columns: new[] { "Id", "Cost", "CreatedBy", "CreatedOn", "IsVisible", "MaxClientUsers", "MaxKeywordsPerProject", "MaxProjects", "MaxTeamUsers", "Name", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                     { new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), 149m, "Migration", new DateTime(2023, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 0, 5000, 50, 0, "Professional", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) } ,
                     { new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), 0m, "Migration", new DateTime(2023, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 0, 0, 0, 0, "Enterprise", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            #endregion

            #region Add Features 

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "Descriptions", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                        //These feature for all plans
                        //Need to assign this feature to for all plan
                        { new Guid("9fdc42fb-df90-4133-a40f-3cb0dbed1eb9"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "All Data Sources", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                        { new Guid("7ee8af73-d7e0-43c2-ad46-9c89536f35b8"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Unlimited Reports", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                        { new Guid("af3991e6-6efa-4ab4-86ba-fb784ae63be2"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Unlimited Client User", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                        { new Guid("3de57595-7b07-4e00-91c5-a97af82b0322"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Unlimited Team User", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },

                        //Only For Growth,Professional,Enterprise
                        //Need to assign this feature to above plan
                        { new Guid("5a6a204b-a062-45b0-87d4-3a9a7d64c76a"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Email WhiteLabel", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                        { new Guid("aa7a9d96-c55b-4ba6-9ea0-2920871eb195"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Setup Training Upto 2 Hours", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                        { new Guid("ae0dd748-b420-4080-bcf3-1eba6053a696"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Setup Training Upto 4 Hours", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                        { new Guid("b5bdeda6-4f7c-49ec-b28a-2eca3d11cf0f"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Setup Training Up to 8 Hours", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },                 
      
                        //For Enterprise only
                        { new Guid("b4fb413f-eced-4a36-bef5-53864ce9ebe0"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Account Manager", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                        { new Guid("4d3a819c-9f73-4404-925b-e77ba1c3f277"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Assissted Migration", "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                });

            #endregion

            #region Add PlanDetails

            migrationBuilder.InsertData(
              table: "PlanDetails",
              columns: new[] { "Id", "CreatedBy", "CreatedOn", "DefaultPlanId", "FeatureID", "UpdatedBy", "UpdatedOn", "Visibility" },
              values: new object[,]
                  {                                                                                                                                                   
                        //Starter
                        { new Guid("69e79d05-29ca-4f30-817c-25880884a281"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"), new Guid("9fdc42fb-df90-4133-a40f-3cb0dbed1eb9"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("7759c1da-ed05-4af3-a051-ade6fcd9b1f8"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"), new Guid("7ee8af73-d7e0-43c2-ad46-9c89536f35b8"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("9daee467-ab37-4980-9932-85c33df6eadd"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"), new Guid("af3991e6-6efa-4ab4-86ba-fb784ae63be2"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("b6a4f9e6-ff40-42c5-a4bb-6079a0f1c3d4"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"), new Guid("3de57595-7b07-4e00-91c5-a97af82b0322"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },

                        //Growth
                        { new Guid("678c3e4f-53ed-4bbd-be47-24bc098c5bf4"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"), new Guid("9fdc42fb-df90-4133-a40f-3cb0dbed1eb9"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("e0ec25be-bc95-438e-b462-579e41232146"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"), new Guid("7ee8af73-d7e0-43c2-ad46-9c89536f35b8"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("537bbf53-baab-4c88-b120-1171837384d6"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"), new Guid("af3991e6-6efa-4ab4-86ba-fb784ae63be2"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("373467b2-0858-4461-b2d1-19c810f59833"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"), new Guid("3de57595-7b07-4e00-91c5-a97af82b0322"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                                       

                        //Professional
                        { new Guid("c20999dc-7ea8-4987-af23-a559e2d4feff"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("9fdc42fb-df90-4133-a40f-3cb0dbed1eb9"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("6a3eab00-bc0d-45fe-af96-29d46bd02294"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("7ee8af73-d7e0-43c2-ad46-9c89536f35b8"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("76ce495b-b777-4c18-96b1-911ef6e6d1cd"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("af3991e6-6efa-4ab4-86ba-fb784ae63be2"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("d73bfcd8-5a13-4380-a69b-339e9751a1de"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("3de57595-7b07-4e00-91c5-a97af82b0322"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },

                        //EnterPrise
                        { new Guid("2f46e86f-f3d4-4db5-b813-abc9fd813033"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("9fdc42fb-df90-4133-a40f-3cb0dbed1eb9"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("7ec3f0ae-55e0-4875-84c7-596c916a90fb"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("7ee8af73-d7e0-43c2-ad46-9c89536f35b8"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("b32bc8d6-60ff-424d-afad-37e5bb88f962"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("af3991e6-6efa-4ab4-86ba-fb784ae63be2"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("10ab9613-61e9-44a2-a853-3f644a600556"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("3de57595-7b07-4e00-91c5-a97af82b0322"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    

                        //Assign all existing feature to Professional
                        { new Guid("570ebdd4-6bfc-48b7-87f0-20d9e5a5dce6"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("eea3ceb5-7325-438c-8c45-58a70a17f400"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("e60b07eb-b2d0-4cac-9ed1-981631cda016"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("4cbfc075-75c5-4375-aedb-bb68055db0e8"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    
                        //Assign all existing feature to Enterprise
                        { new Guid("effdfb5b-3499-488d-b9d9-6efc0e6e41c7"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("66de7f38-8395-46cf-bd0c-264dab469952"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("a0dc5bb0-948c-42db-bc69-56937a4fe4f4"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("cba2bedb-89e0-4b98-9beb-81b88b823489"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },

                        //Newly added Feature for growth professional enterprise
                        { new Guid("4d91436f-4cdb-4e5c-ad9c-e5a1c2d53836"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"), new Guid("5a6a204b-a062-45b0-87d4-3a9a7d64c76a"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("c6eaa7df-dead-49c8-82a8-06a9dcaf1054"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"), new Guid("aa7a9d96-c55b-4ba6-9ea0-2920871eb195"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        
                        { new Guid("5c81dfe8-3d89-4fe9-88d4-52631f86f73a"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("5a6a204b-a062-45b0-87d4-3a9a7d64c76a"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("91e418b7-6c52-40d9-95b9-fda306e1e2fa"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"), new Guid("ae0dd748-b420-4080-bcf3-1eba6053a696"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        
                        { new Guid("4c3a76d0-810a-4013-a174-19f62cdc5746"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("5a6a204b-a062-45b0-87d4-3a9a7d64c76a"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("e735893e-2455-45b4-92e3-55375200d451"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("b5bdeda6-4f7c-49ec-b28a-2eca3d11cf0f"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },

                        //Newly added feature for enterprise
                        { new Guid("b88bb5b4-69fa-4554-bae3-4583441e1563"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("b4fb413f-eced-4a36-bef5-53864ce9ebe0"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("cf8c1091-7670-4e4a-b2c1-d921138659de"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"), new Guid("4d3a819c-9f73-4404-925b-e77ba1c3f277"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },


                        //Assign All feature to free plan
                        { new Guid("efa58d88-f299-45ac-b3c8-ceef9af06645"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"), new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("9d7ccb84-b283-4e89-a76d-0bbaebc8eff3"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"), new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("6739b908-ad25-4dcb-83a3-ee67acfcc2b0"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"), new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("73820e1b-46bb-4817-a146-fc8433ec61ce"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"), new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },

                        { new Guid("21df5bb4-23fa-464d-b953-8ebc72851a88"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"), new Guid("9fdc42fb-df90-4133-a40f-3cb0dbed1eb9"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("c97f9d9b-7bc7-40ab-b3d8-7442712f43ca"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"), new Guid("7ee8af73-d7e0-43c2-ad46-9c89536f35b8"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("e0f79587-6e25-4450-ac4f-91ab623f60f6"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"), new Guid("af3991e6-6efa-4ab4-86ba-fb784ae63be2"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                        { new Guid("e5b488ce-2000-43e2-b3a4-fd9113a260a4"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("BA57022F-EB81-4EB5-B590-EF6D418B1DB9"), new Guid("3de57595-7b07-4e00-91c5-a97af82b0322"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },

              });

            #endregion

            #region Delete Feature assigning from planDetails Like (facebook integration, instagram integration)

            //Remove all integration text from startup and growth (Like facebook integration, instagram integration)
            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("FB37A693-0FEC-40C2-B77B-738407A38EB4"));

            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("5A66F69D-352B-4D6E-9E08-83E421785456"));

            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("2EA0B2EB-B1EA-470E-A6BB-8EBF3B7F023B"));

            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("34091AFF-4BF0-419B-9975-98121EE34C20"));

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("F13E1E86-A095-4392-9106-A78C8BCBA87E"));

            migrationBuilder.DeleteData(
           table: "PlanDetails",
           keyColumn: "Id",
           keyValue: new Guid("C488BDED-F58E-4896-9E65-ADD7DED51FD6"));

            migrationBuilder.DeleteData(
           table: "PlanDetails",
           keyColumn: "Id",
           keyValue: new Guid("13BBCD8B-4C4E-4FBE-A633-BE877F2A7AF6"));

            //Delete for Growth
            migrationBuilder.DeleteData(
             table: "PlanDetails",
             keyColumn: "Id",
             keyValue: new Guid("30CF91B2-298F-4914-A389-478A6CCDBF0B"));

            migrationBuilder.DeleteData(
           table: "PlanDetails",
           keyColumn: "Id",
           keyValue: new Guid("F597DE30-E986-4BF8-A4A7-5CEE04C99A74"));

            migrationBuilder.DeleteData(
           table: "PlanDetails",
           keyColumn: "Id",
           keyValue: new Guid("EE432552-D73B-4F14-B2E4-6E4D60762E4E"));

            migrationBuilder.DeleteData(
           table: "PlanDetails",
           keyColumn: "Id",
           keyValue: new Guid("6F6C4C83-428C-423C-8229-B78CA2E2B6C7"));

            migrationBuilder.DeleteData(
           table: "PlanDetails",
           keyColumn: "Id",
           keyValue: new Guid("E1AC379B-C116-41A5-8614-C01ADCB484D3"));

            migrationBuilder.DeleteData(
           table: "PlanDetails",
           keyColumn: "Id",
           keyValue: new Guid("EB513D35-CD9C-4858-B2B7-C23791F05248"));

            migrationBuilder.DeleteData(
           table: "PlanDetails",
           keyColumn: "Id",
           keyValue: new Guid("3D361B1E-61E7-4859-9B71-F68C651FDEFA"));

            #endregion

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            #region Update Plan

            migrationBuilder.UpdateData(
                table: "DefaultPlans",
                keyColumn: "Id",
                keyValue: new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"),
                columns: new[] { "Name", "MaxProjects", "MaxKeywordsPerProject" },
                values: new object[] { "STARTUP", "3", "50" });

            migrationBuilder.UpdateData(
               table: "DefaultPlans",
               keyColumn: "Id",
               keyValue: new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
               columns: new[] { "Name", "MaxProjects", "MaxKeywordsPerProject" },
               values: new object[] { "AGENCY", "10", "100" });

            migrationBuilder.DeleteData(
            table: "DefaultPlans",
            keyColumn: "Id",
            keyValue: new Guid("3a5e8f7d-19e6-4be7-869a-1f0ed8c907ff"));

            migrationBuilder.DeleteData(
               table: "DefaultPlans",
               keyColumn: "Id",
               keyValue: new Guid("9f3cf9f2-f4e0-445f-8db5-0697161cbcca"));

            #endregion

            #region Delete Feature

            //Delete features
            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("9fdc42fb-df90-4133-a40f-3cb0dbed1eb9"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("7ee8af73-d7e0-43c2-ad46-9c89536f35b8"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("af3991e6-6efa-4ab4-86ba-fb784ae63be2"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("3de57595-7b07-4e00-91c5-a97af82b0322"));


            migrationBuilder.DeleteData(
              table: "Features",
              keyColumn: "Id",
              keyValue: new Guid("5a6a204b-a062-45b0-87d4-3a9a7d64c76a"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("aa7a9d96-c55b-4ba6-9ea0-2920871eb195"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("ae0dd748-b420-4080-bcf3-1eba6053a696"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("b5bdeda6-4f7c-49ec-b28a-2eca3d11cf0f"));

            migrationBuilder.DeleteData(
              table: "Features",
              keyColumn: "Id",
              keyValue: new Guid("b4fb413f-eced-4a36-bef5-53864ce9ebe0"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("4d3a819c-9f73-4404-925b-e77ba1c3f277"));

            #endregion

            #region Delete PlanDetails

            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("69e79d05-29ca-4f30-817c-25880884a281"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("7759c1da-ed05-4af3-a051-ade6fcd9b1f8"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("9daee467-ab37-4980-9932-85c33df6eadd"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("b6a4f9e6-ff40-42c5-a4bb-6079a0f1c3d4"));


            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("678c3e4f-53ed-4bbd-be47-24bc098c5bf4"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("e0ec25be-bc95-438e-b462-579e41232146"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("537bbf53-baab-4c88-b120-1171837384d6"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("373467b2-0858-4461-b2d1-19c810f59833"));


            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("c20999dc-7ea8-4987-af23-a559e2d4feff"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("6a3eab00-bc0d-45fe-af96-29d46bd02294"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("76ce495b-b777-4c18-96b1-911ef6e6d1cd"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("d73bfcd8-5a13-4380-a69b-339e9751a1de"));


            //EnterPrise

            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("2f46e86f-f3d4-4db5-b813-abc9fd813033"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("7ec3f0ae-55e0-4875-84c7-596c916a90fb"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("b32bc8d6-60ff-424d-afad-37e5bb88f962"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("10ab9613-61e9-44a2-a853-3f644a600556"));


            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("4d91436f-4cdb-4e5c-ad9c-e5a1c2d53836"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("c6eaa7df-dead-49c8-82a8-06a9dcaf1054"));




            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("5c81dfe8-3d89-4fe9-88d4-52631f86f73a"));



            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("91e418b7-6c52-40d9-95b9-fda306e1e2fa"));


            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("4c3a76d0-810a-4013-a174-19f62cdc5746"));


            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("e735893e-2455-45b4-92e3-55375200d451"));


            migrationBuilder.DeleteData(
            table: "PlanDetails",
            keyColumn: "Id",
            keyValue: new Guid("b88bb5b4-69fa-4554-bae3-4583441e1563"));

            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("cf8c1091-7670-4e4a-b2c1-d921138659de"));

                       
            
            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("570ebdd4-6bfc-48b7-87f0-20d9e5a5dce6"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("eea3ceb5-7325-438c-8c45-58a70a17f400"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("e60b07eb-b2d0-4cac-9ed1-981631cda016"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("4cbfc075-75c5-4375-aedb-bb68055db0e8"));


            migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("effdfb5b-3499-488d-b9d9-6efc0e6e41c7"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("66de7f38-8395-46cf-bd0c-264dab469952"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("a0dc5bb0-948c-42db-bc69-56937a4fe4f4"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("cba2bedb-89e0-4b98-9beb-81b88b823489"));


                migrationBuilder.DeleteData(
                table: "PlanDetails",
                keyColumn: "Id",
                keyValue: new Guid("efa58d88-f299-45ac-b3c8-ceef9af06645"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("9d7ccb84-b283-4e89-a76d-0bbaebc8eff3"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("6739b908-ad25-4dcb-83a3-ee67acfcc2b0"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("73820e1b-46bb-4817-a146-fc8433ec61ce"));



            migrationBuilder.DeleteData(
              table: "PlanDetails",
              keyColumn: "Id",
              keyValue: new Guid("21df5bb4-23fa-464d-b953-8ebc72851a88"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("c97f9d9b-7bc7-40ab-b3d8-7442712f43ca"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("e0f79587-6e25-4450-ac4f-91ab623f60f6"));
            migrationBuilder.DeleteData(
               table: "PlanDetails",
               keyColumn: "Id",
               keyValue: new Guid("e5b488ce-2000-43e2-b3a4-fd9113a260a4"));

            #endregion

            #region Add Feature assigning from planDetails Like (facebook integration, instagram integration)

            migrationBuilder.InsertData(
             table: "PlanDetails",
             columns: new[] { "Id", "CreatedBy", "CreatedOn", "DefaultPlanId", "FeatureID", "UpdatedBy", "UpdatedOn", "Visibility" },
             values: new object[,]
             {
                  { new Guid("fb37a693-0fec-40c2-b77b-738407a38eb4"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("5a66f69d-352b-4d6e-9e08-83e421785456"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("2ea0b2eb-b1ea-470e-a6bb-8ebf3b7f023b"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("f558f102-2408-4a80-b7a9-e195adefba55"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("34091aff-4bf0-419b-9975-98121ee34c20"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("f13e1e86-a095-4392-9106-a78c8bcba87e"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("c488bded-f58e-4896-9e65-add7ded51fd6"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("13bbcd8b-4c4e-4fbe-a633-be877f2a7af6"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"), new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },

                  { new Guid("30cf91b2-298f-4914-a389-478a6ccdbf0b"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("f597de30-e986-4bf8-a4a7-5cee04c99a74"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("ee432552-d73b-4f14-b2e4-6e4d60762e4e"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("f558f102-2408-4a80-b7a9-e195adefba55"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("6f6c4c83-428c-423c-8229-b78ca2e2b6c7"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("e1ac379b-c116-41a5-8614-c01adcb484d3"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("eb513d35-cd9c-4858-b2b7-c23791f05248"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                  { new Guid("3d361b1e-61e7-4859-9b71-f68c651fdefa"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"), new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"), "Migration", new DateTime(2023, 09, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
             });

            #endregion

        }
    }
}
