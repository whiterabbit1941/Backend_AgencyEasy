using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class WebUrlNotRequiredInCampaign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WebUrl",
                table: "Campaigns",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 250);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WebUrl",
                table: "Campaigns",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
