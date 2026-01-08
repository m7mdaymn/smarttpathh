using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class scanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "9666e21f-06d3-422d-a30b-b2a87aab9c8a");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "a3c83ebc-6933-4ac6-9987-fda6f01e62a8", null, new DateTime(2026, 1, 7, 13, 42, 57, 29, DateTimeKind.Utc).AddTicks(7972), "mohamed@test.com", true, "System Admin", "$2a$13$Q5./HtXq/16oj2gRr6nKAerdp0x88NogjBvFBBEB2cZidC9TRDldi", "01000000000", "superadmin", new DateTime(2026, 1, 7, 13, 42, 57, 29, DateTimeKind.Utc).AddTicks(8220) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "a3c83ebc-6933-4ac6-9987-fda6f01e62a8");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "9666e21f-06d3-422d-a30b-b2a87aab9c8a", null, new DateTime(2026, 1, 7, 12, 6, 24, 294, DateTimeKind.Utc).AddTicks(1887), "mohamed@test.com", true, "System Admin", "$2a$13$trzUGxEqUnrSxqKKYhRRKuiLrPOpON/A2bm2wgDEzXwd4FqlWcyqm", "01000000000", "superadmin", new DateTime(2026, 1, 7, 12, 6, 24, 294, DateTimeKind.Utc).AddTicks(2070) });
        }
    }
}
