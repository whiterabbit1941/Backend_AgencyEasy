using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddStripeProductIdandPriceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "priceId",
                table: "Plans",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripeProductId",
                table: "Plans",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "priceId",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "stripeProductId",
                table: "Plans");
        }
    }
}
