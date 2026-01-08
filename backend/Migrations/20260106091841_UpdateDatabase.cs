using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferUsages_Offers_OfferId",
                table: "OfferUsages");

            migrationBuilder.AddColumn<bool>(
                name: "CanAddOffers",
                table: "Merchants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasNotificationsEnabled",
                table: "Merchants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasUnlimitedUsers",
                table: "Merchants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LoyaltyCardsTemporarilySuspended",
                table: "Merchants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlanChangeRequest",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanChangeRequestedAt",
                table: "Merchants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferUsages_Offers_OfferId",
                table: "OfferUsages",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferUsages_Offers_OfferId",
                table: "OfferUsages");

            migrationBuilder.DropColumn(
                name: "CanAddOffers",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "HasNotificationsEnabled",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "HasUnlimitedUsers",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "LoyaltyCardsTemporarilySuspended",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "PlanChangeRequest",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "PlanChangeRequestedAt",
                table: "Merchants");

            migrationBuilder.AddForeignKey(
                name: "FK_OfferUsages_Offers_OfferId",
                table: "OfferUsages",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
