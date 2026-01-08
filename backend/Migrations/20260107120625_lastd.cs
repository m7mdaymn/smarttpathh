using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class lastd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "0b02558f-7dbf-46d5-84f5-e3f0fcb5c03c");

            migrationBuilder.AlterColumn<string>(
                name: "MerchantId",
                table: "CarPhotos",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "9666e21f-06d3-422d-a30b-b2a87aab9c8a", null, new DateTime(2026, 1, 7, 12, 6, 24, 294, DateTimeKind.Utc).AddTicks(1887), "mohamed@test.com", true, "System Admin", "$2a$13$trzUGxEqUnrSxqKKYhRRKuiLrPOpON/A2bm2wgDEzXwd4FqlWcyqm", "01000000000", "superadmin", new DateTime(2026, 1, 7, 12, 6, 24, 294, DateTimeKind.Utc).AddTicks(2070) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "9666e21f-06d3-422d-a30b-b2a87aab9c8a");

            migrationBuilder.AlterColumn<string>(
                name: "MerchantId",
                table: "CarPhotos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "0b02558f-7dbf-46d5-84f5-e3f0fcb5c03c", null, new DateTime(2026, 1, 7, 9, 2, 45, 832, DateTimeKind.Utc).AddTicks(1616), "mohamed@test.com", true, "System Admin", "$2a$13$PJPYcIRI8JH4gCNGVmfzPOp6.yjAHPDxWugNOYbHp2SzccFLF98CG", "01000000000", "superadmin", new DateTime(2026, 1, 7, 9, 2, 45, 832, DateTimeKind.Utc).AddTicks(1827) });
        }
    }
}
