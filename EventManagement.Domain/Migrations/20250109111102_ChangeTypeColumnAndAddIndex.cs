using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class ChangeTypeColumnAndAddIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "TaskId",
                table: "Serps",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Add an index on TaskId
            migrationBuilder.CreateIndex(
                name: "IX_Serps_TaskId", // Index name
                table: "Serps",
                column: "TaskId"); // Column to index

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the index on TaskId
            migrationBuilder.DropIndex(
                name: "IX_Serps_TaskId", // Index name
                table: "Serps");

            migrationBuilder.AlterColumn<string>(
                name: "TaskId",
                table: "Serps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
