using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "010d122e-5b20-41b9-b46a-d2d7ff5ede2e");

            migrationBuilder.AddColumn<string>(
                name: "MerchantComment",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RewardDescriptionAr",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlateNumber",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "b84acf20-3574-463c-907d-fdc7c8157f62", null, new DateTime(2026, 1, 6, 10, 30, 40, 951, DateTimeKind.Utc).AddTicks(6760), "mohamed@test.com", true, "System Admin", "$2a$13$AsHM7QhniO4b1uF08Rjc5e/xZ/1ndsVzi/T2zhcVltMQblaYb9EI6", "01000000000", "superadmin", new DateTime(2026, 1, 6, 10, 30, 40, 951, DateTimeKind.Utc).AddTicks(6949) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "b84acf20-3574-463c-907d-fdc7c8157f62");

            migrationBuilder.DropColumn(
                name: "MerchantComment",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "RewardDescriptionAr",
                table: "MerchantSettings");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PlateNumber",
                table: "Customers");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "010d122e-5b20-41b9-b46a-d2d7ff5ede2e", null, new DateTime(2026, 1, 6, 10, 15, 36, 328, DateTimeKind.Utc).AddTicks(7134), "mohamed@test.com", true, "System Admin", "$2a$13$ZRURk8FUkYPT4s8bVCSJDOmPPPZOga8nI/o3tNGy0ftoisUzT2Vnu", "01000000000", "superadmin", new DateTime(2026, 1, 6, 10, 15, 36, 328, DateTimeKind.Utc).AddTicks(7344) });
        }
    }
}
