using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddedDomainWhitelabelTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DomainWhitelabels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DomainName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlternateDomainName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CnameType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CnameHost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CnamePointsTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateARN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistributionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainWhitelabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainWhitelabels_Companys_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainWhitelabels_CompanyID",
                table: "DomainWhitelabels",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_DomainWhitelabels_DistributionId",
                table: "DomainWhitelabels",
                column: "DistributionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainWhitelabels");
        }
    }
}
