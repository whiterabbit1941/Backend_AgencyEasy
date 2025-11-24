using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace EventManagement.Domain.Migrations
{
    public partial class EmailWhiteLabel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "EmailWhitelabels",
               columns: table => new
               {
                   Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                   CompanyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                   DomainID = table.Column<int>(type: "int", nullable: false),
                   DomainName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   CnameHost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   CnameType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   CnamePointsTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   DomainKey1Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   DomainKey1PointsTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   DomainKey2Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   DomainKey2PointsTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   IsVerify = table.Column<bool>(type: "bit", nullable: false),
                   CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                   CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                   UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                   UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_EmailWhitelabels", x => x.Id);
                   table.ForeignKey(
                       name: "FK_EmailWhitelabels_Companys_CompanyID",
                       column: x => x.CompanyID,
                       principalTable: "Companys",
                       principalColumn: "Id",
                       onDelete: ReferentialAction.Cascade);
               });

            migrationBuilder.CreateIndex(
                name: "IX_EmailWhitelabels_CompanyID",
                table: "EmailWhitelabels",
                column: "CompanyID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EmailWhitelabels");
        }
    }
}
