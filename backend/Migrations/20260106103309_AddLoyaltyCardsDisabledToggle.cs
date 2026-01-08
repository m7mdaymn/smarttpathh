using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyCardsDisabledToggle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "b84acf20-3574-463c-907d-fdc7c8157f62");

            migrationBuilder.AddColumn<bool>(
                name: "LoyaltyCardsDisabled",
                table: "MerchantSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "1846ee9d-e252-4b80-9a69-e34998e676d1", null, new DateTime(2026, 1, 6, 10, 33, 8, 960, DateTimeKind.Utc).AddTicks(177), "mohamed@test.com", true, "System Admin", "$2a$13$/rTbJ0Sh8ZubWQ3W7eSneOqkuf9pEL6y.4jDDdoPQAVIMqL1NRV6G", "01000000000", "superadmin", new DateTime(2026, 1, 6, 10, 33, 8, 960, DateTimeKind.Utc).AddTicks(368) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "1846ee9d-e252-4b80-9a69-e34998e676d1");

            migrationBuilder.DropColumn(
                name: "LoyaltyCardsDisabled",
                table: "MerchantSettings");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "b84acf20-3574-463c-907d-fdc7c8157f62", null, new DateTime(2026, 1, 6, 10, 30, 40, 951, DateTimeKind.Utc).AddTicks(6760), "mohamed@test.com", true, "System Admin", "$2a$13$AsHM7QhniO4b1uF08Rjc5e/xZ/1ndsVzi/T2zhcVltMQblaYb9EI6", "01000000000", "superadmin", new DateTime(2026, 1, 6, 10, 30, 40, 951, DateTimeKind.Utc).AddTicks(6949) });
        }
    }
}
