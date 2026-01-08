using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "7d92f52f-2404-43f4-b282-d2cec35d88a8");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "010d122e-5b20-41b9-b46a-d2d7ff5ede2e", null, new DateTime(2026, 1, 6, 10, 15, 36, 328, DateTimeKind.Utc).AddTicks(7134), "mohamed@test.com", true, "System Admin", "$2a$13$ZRURk8FUkYPT4s8bVCSJDOmPPPZOga8nI/o3tNGy0ftoisUzT2Vnu", "01000000000", "superadmin", new DateTime(2026, 1, 6, 10, 15, 36, 328, DateTimeKind.Utc).AddTicks(7344) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "010d122e-5b20-41b9-b46a-d2d7ff5ede2e");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "7d92f52f-2404-43f4-b282-d2cec35d88a8", null, new DateTime(2026, 1, 6, 10, 14, 49, 160, DateTimeKind.Utc).AddTicks(8623), "mohamed@test.com", true, "System Admin", "$2a$13$ayppFJWiUxpDNNQszwsYMObwx19lz9iRCQuO9gJuZeRtmnP33BLYm", "01000000000", "superadmin", new DateTime(2026, 1, 6, 10, 14, 49, 160, DateTimeKind.Utc).AddTicks(8808) });
        }
    }
}
