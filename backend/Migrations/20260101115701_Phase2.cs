using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Phase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedOfferId",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountApplied",
                table: "WashHistories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "QRCodeGeneratedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    MinimumWashes = table.Column<int>(type: "int", nullable: true),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsageLimit = table.Column<int>(type: "int", nullable: true),
                    UsagePerCustomer = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimesUsed = table.Column<int>(type: "int", nullable: false),
                    SendNotification = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offers_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WashHistoryServices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WashHistoryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServiceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WashHistoryServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WashHistoryServices_MerchantServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "MerchantServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WashHistoryServices_WashHistories_WashHistoryId",
                        column: x => x.WashHistoryId,
                        principalTable: "WashHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfferUsages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OfferId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WashHistoryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DiscountApplied = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferUsages_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferUsages_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfferUsages_WashHistories_WashHistoryId",
                        column: x => x.WashHistoryId,
                        principalTable: "WashHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_MerchantId",
                table: "Offers",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferUsages_CustomerId",
                table: "OfferUsages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferUsages_OfferId",
                table: "OfferUsages",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferUsages_WashHistoryId",
                table: "OfferUsages",
                column: "WashHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WashHistoryServices_ServiceId",
                table: "WashHistoryServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_WashHistoryServices_WashHistoryId",
                table: "WashHistoryServices",
                column: "WashHistoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfferUsages");

            migrationBuilder.DropTable(
                name: "WashHistoryServices");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropColumn(
                name: "AppliedOfferId",
                table: "WashHistories");

            migrationBuilder.DropColumn(
                name: "DiscountApplied",
                table: "WashHistories");

            migrationBuilder.DropColumn(
                name: "QRCodeGeneratedAt",
                table: "Customers");
        }
    }
}
