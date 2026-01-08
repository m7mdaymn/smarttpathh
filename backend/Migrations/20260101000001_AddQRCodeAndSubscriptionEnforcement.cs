using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCodeAndSubscriptionEnforcement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "QRCodeGeneratedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true,
                comment: "When the QR code was generated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRCodeGeneratedAt",
                table: "Customers");
        }
    }
}
