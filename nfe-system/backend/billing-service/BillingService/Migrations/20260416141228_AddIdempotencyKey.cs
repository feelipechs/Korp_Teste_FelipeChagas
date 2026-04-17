using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IdempotencyKey",
                table: "Invoices",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_IdempotencyKey",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Invoices");
        }
    }
}
