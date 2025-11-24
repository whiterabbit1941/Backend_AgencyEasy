using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddCompanyIdInProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyID",
                table: "Products",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyID",
                table: "Products",
                column: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Companys_CompanyID",
                table: "Products",
                column: "CompanyID",
                principalTable: "Companys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Companys_CompanyID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CompanyID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Products");
        }
    }
}
