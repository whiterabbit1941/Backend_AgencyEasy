using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class NewCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Companys_CompanyID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Companys_CompanyID",
                table: "Campaigns");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyID",
                table: "Campaigns",
                nullable: true,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyID",
                table: "Auditss",
                nullable: true,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyID",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Role",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Auditss_CompanyID",
                table: "Auditss",
                column: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Companys_CompanyID",
                table: "AspNetUsers",
                column: "CompanyID",
                principalTable: "Companys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auditss_Companys_CompanyID",
                table: "Auditss",
                column: "CompanyID",
                principalTable: "Companys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Companys_CompanyID",
                table: "Campaigns",
                column: "CompanyID",
                principalTable: "Companys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Companys_CompanyID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Auditss_Companys_CompanyID",
                table: "Auditss");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Companys_CompanyID",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Auditss_CompanyID",
                table: "Auditss");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Auditss");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyID",
                table: "Campaigns",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyID",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Companys_CompanyID",
                table: "AspNetUsers",
                column: "CompanyID",
                principalTable: "Companys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Companys_CompanyID",
                table: "Campaigns",
                column: "CompanyID",
                principalTable: "Companys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
