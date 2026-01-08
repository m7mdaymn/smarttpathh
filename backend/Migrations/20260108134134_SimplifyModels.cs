using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_Merchants_MerchantId",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "WashHistories");

            migrationBuilder.DropColumn(
                name: "AppliedOfferId",
                table: "WashHistories");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "WashHistories");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "MerchantComment",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "CustomRewardMessage",
                table: "MerchantSettings");

            migrationBuilder.DropColumn(
                name: "LoyaltyCardsDisabled",
                table: "MerchantSettings");

            migrationBuilder.DropColumn(
                name: "CanAddOffers",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "CustomLogo",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "HasUnlimitedUsers",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "Logo",
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

            migrationBuilder.RenameColumn(
                name: "DiscountApplied",
                table: "WashHistories",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Rewards",
                newName: "LoyaltyCardId");

            migrationBuilder.RenameColumn(
                name: "WalletBalance",
                table: "Customers",
                newName: "TotalSpent");

            migrationBuilder.RenameColumn(
                name: "LoyaltyPoints",
                table: "Customers",
                newName: "TotalRewardsEarned");

            migrationBuilder.AddColumn<string>(
                name: "CustomerComment",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceDescription",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RewardQRCode",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimedByMerchantId",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "TotalRevenue",
                table: "Merchants",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "TotalRewardsGiven",
                table: "Merchants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "RewardQRCode",
                table: "LoyaltyCards",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsRewardEarned",
                table: "LoyaltyCards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RewardEarnedAt",
                table: "LoyaltyCards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarPhoto",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWashAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalRewardsClaimed",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_Merchants_MerchantId",
                table: "Rewards",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_Merchants_MerchantId",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "CustomerComment",
                table: "WashHistories");

            migrationBuilder.DropColumn(
                name: "ServiceDescription",
                table: "WashHistories");

            migrationBuilder.DropColumn(
                name: "TotalRewardsGiven",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "IsRewardEarned",
                table: "LoyaltyCards");

            migrationBuilder.DropColumn(
                name: "RewardEarnedAt",
                table: "LoyaltyCards");

            migrationBuilder.DropColumn(
                name: "CarPhoto",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LastWashAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TotalRewardsClaimed",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "WashHistories",
                newName: "DiscountApplied");

            migrationBuilder.RenameColumn(
                name: "LoyaltyCardId",
                table: "Rewards",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "TotalSpent",
                table: "Customers",
                newName: "WalletBalance");

            migrationBuilder.RenameColumn(
                name: "TotalRewardsEarned",
                table: "Customers",
                newName: "LoyaltyPoints");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "WashHistories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AppliedOfferId",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RewardQRCode",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClaimedByMerchantId",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantComment",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Rewards",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CustomRewardMessage",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LoyaltyCardsDisabled",
                table: "MerchantSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalRevenue",
                table: "Merchants",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "CanAddOffers",
                table: "Merchants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomLogo",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasUnlimitedUsers",
                table: "Merchants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "RewardQRCode",
                table: "LoyaltyCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_Merchants_MerchantId",
                table: "Rewards",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
