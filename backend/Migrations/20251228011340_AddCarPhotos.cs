using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCarPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RewardDescription",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "غسلة مجانية",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateWelcome",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "مرحباً بك في مغسلتنا",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateRewardClose",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "أنت قريب من الحصول على المكافأة",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateRemaining",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "باقي لك {0} غسلات للحصول على المكافأة",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomRewardMessage",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "مبروك! لقد حصلت على مكافأة",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomBusinessTagline",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "أفضل غسيل سيارات",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CarPhotos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarPhotos_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarPhotos_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarPhotos_CustomerId",
                table: "CarPhotos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CarPhotos_MerchantId",
                table: "CarPhotos",
                column: "MerchantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarPhotos");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RewardDescription",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "غسلة مجانية");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateWelcome",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "مرحباً بك في مغسلتنا");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateRewardClose",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "أنت قريب من الحصول على المكافأة");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateRemaining",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "باقي لك {0} غسلات للحصول على المكافأة");

            migrationBuilder.AlterColumn<string>(
                name: "CustomRewardMessage",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "مبروك! لقد حصلت على مكافأة");

            migrationBuilder.AlterColumn<string>(
                name: "CustomBusinessTagline",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "أفضل غسيل سيارات");
        }
    }
}
