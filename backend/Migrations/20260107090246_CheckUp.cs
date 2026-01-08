using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class CheckUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "0ab63471-e996-4a98-a930-a7d73f99153a");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClaimedAt",
                table: "Rewards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimedByMerchantId",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantId",
                table: "Rewards",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RewardQRCode",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaused",
                table: "LoyaltyCards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRewardClaimed",
                table: "LoyaltyCards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RewardClaimedAt",
                table: "LoyaltyCards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RewardQRCode",
                table: "LoyaltyCards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "0b02558f-7dbf-46d5-84f5-e3f0fcb5c03c", null, new DateTime(2026, 1, 7, 9, 2, 45, 832, DateTimeKind.Utc).AddTicks(1616), "mohamed@test.com", true, "System Admin", "$2a$13$PJPYcIRI8JH4gCNGVmfzPOp6.yjAHPDxWugNOYbHp2SzccFLF98CG", "01000000000", "superadmin", new DateTime(2026, 1, 7, 9, 2, 45, 832, DateTimeKind.Utc).AddTicks(1827) });

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_MerchantId",
                table: "Rewards",
                column: "MerchantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_Merchants_MerchantId",
                table: "Rewards",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_Merchants_MerchantId",
                table: "Rewards");

            migrationBuilder.DropIndex(
                name: "IX_Rewards_MerchantId",
                table: "Rewards");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "0b02558f-7dbf-46d5-84f5-e3f0fcb5c03c");

            migrationBuilder.DropColumn(
                name: "ClaimedAt",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "ClaimedByMerchantId",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "RewardQRCode",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "IsPaused",
                table: "LoyaltyCards");

            migrationBuilder.DropColumn(
                name: "IsRewardClaimed",
                table: "LoyaltyCards");

            migrationBuilder.DropColumn(
                name: "RewardClaimedAt",
                table: "LoyaltyCards");

            migrationBuilder.DropColumn(
                name: "RewardQRCode",
                table: "LoyaltyCards");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "0ab63471-e996-4a98-a930-a7d73f99153a", null, new DateTime(2026, 1, 6, 19, 30, 49, 464, DateTimeKind.Utc).AddTicks(7655), "mohamed@test.com", true, "System Admin", "$2a$13$LHd73uJ8jLVeNIe4T81cg.iDTcRifkho6d9AWWZBHbMoF1y/dkQcC", "01000000000", "superadmin", new DateTime(2026, 1, 6, 19, 30, 49, 464, DateTimeKind.Utc).AddTicks(8012) });

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
