using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class CustomerMerchantLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "1846ee9d-e252-4b80-9a69-e34998e676d1");

            // Drop existing index if it exists (from partial migration)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_MerchantId' AND object_id = OBJECT_ID('Customers'))
                BEGIN
                    DROP INDEX [IX_Customers_MerchantId] ON [Customers];
                END
            ");

            // Delete customers without valid MerchantId or with empty MerchantId
            migrationBuilder.Sql(@"
                DELETE FROM [Customers] WHERE [MerchantId] IS NULL OR [MerchantId] = '' OR [MerchantId] NOT IN (SELECT [Id] FROM [Merchants]);
            ");

            migrationBuilder.AddColumn<DateTime>(
                name: "QRCodeGeneratedAt",
                table: "Merchants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QRCodeImageUrl",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationCode",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "MerchantId",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "0ab63471-e996-4a98-a930-a7d73f99153a", null, new DateTime(2026, 1, 6, 19, 30, 49, 464, DateTimeKind.Utc).AddTicks(7655), "mohamed@test.com", true, "System Admin", "$2a$13$LHd73uJ8jLVeNIe4T81cg.iDTcRifkho6d9AWWZBHbMoF1y/dkQcC", "01000000000", "superadmin", new DateTime(2026, 1, 6, 19, 30, 49, 464, DateTimeKind.Utc).AddTicks(8012) });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_MerchantId",
                table: "Customers",
                column: "MerchantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_MerchantId",
                table: "Customers");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "0ab63471-e996-4a98-a930-a7d73f99153a");

            migrationBuilder.DropColumn(
                name: "QRCodeGeneratedAt",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "QRCodeImageUrl",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "RegistrationCode",
                table: "Merchants");

            migrationBuilder.AlterColumn<string>(
                name: "MerchantId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "1846ee9d-e252-4b80-9a69-e34998e676d1", null, new DateTime(2026, 1, 6, 10, 33, 8, 960, DateTimeKind.Utc).AddTicks(177), "mohamed@test.com", true, "System Admin", "$2a$13$/rTbJ0Sh8ZubWQ3W7eSneOqkuf9pEL6y.4jDDdoPQAVIMqL1NRV6G", "01000000000", "superadmin", new DateTime(2026, 1, 6, 10, 33, 8, 960, DateTimeKind.Utc).AddTicks(368) });
        }
    }
}
