using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class ChangeStripePayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "StripePayments");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "StripePayments",
                nullable: true,
                oldClrType: typeof(double));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StripePayments",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMode",
                table: "StripePayments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentId",
                table: "StripePayments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "PaymentMode",
                table: "StripePayments");

            migrationBuilder.DropColumn(
                name: "StripePaymentId",
                table: "StripePayments");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "StripePayments",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "StripePayments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanId",
                table: "StripePayments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "StripePayments",
                nullable: false,
                defaultValue: 0);
        }
    }
}
