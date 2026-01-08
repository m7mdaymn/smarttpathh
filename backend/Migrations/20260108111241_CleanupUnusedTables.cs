using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class CleanupUnusedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys safely (they may or may not exist)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CarPhotos_Merchants_MerchantId')
                    ALTER TABLE [CarPhotos] DROP CONSTRAINT [FK_CarPhotos_Merchants_MerchantId];
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Customers_Merchants_MerchantId')
                    ALTER TABLE [Customers] DROP CONSTRAINT [FK_Customers_Merchants_MerchantId];
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Rewards_Merchants_MerchantId')
                    ALTER TABLE [Rewards] DROP CONSTRAINT [FK_Rewards_Merchants_MerchantId];
            ");

            // Drop unused tables if they exist
            migrationBuilder.Sql(@"
                IF OBJECT_ID('CustomerSubscriptions', 'U') IS NOT NULL DROP TABLE [CustomerSubscriptions];
                IF OBJECT_ID('OfferUsages', 'U') IS NOT NULL DROP TABLE [OfferUsages];
                IF OBJECT_ID('WalletTransactions', 'U') IS NOT NULL DROP TABLE [WalletTransactions];
                IF OBJECT_ID('WashHistoryServices', 'U') IS NOT NULL DROP TABLE [WashHistoryServices];
                IF OBJECT_ID('Offers', 'U') IS NOT NULL DROP TABLE [Offers];
                IF OBJECT_ID('MerchantServices', 'U') IS NOT NULL DROP TABLE [MerchantServices];
            ");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "a3c83ebc-6933-4ac6-9987-fda6f01e62a8");

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
                name: "AppliedOfferId",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Avatar",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
                name: "MerchantId",
                table: "Rewards",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MerchantComment",
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

            migrationBuilder.AlterColumn<string>(
                name: "RewardDescription",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "غسلة مجانية");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateWelcome",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "مرحباً بك في مغسلتنا");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateRewardClose",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "أنت قريب من الحصول على المكافأة");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateRemaining",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "باقي لك {0} غسلات للحصول على المكافأة");

            migrationBuilder.AlterColumn<string>(
                name: "CustomRewardMessage",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "مبروك! لقد حصلت على مكافأة");

            migrationBuilder.AlterColumn<string>(
                name: "CustomBusinessTagline",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "أفضل غسيل سيارات");

            migrationBuilder.AlterColumn<string>(
                name: "QRCodeImageUrl",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlanChangeRequest",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomLogo",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RewardQRCode",
                table: "LoyaltyCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlateNumber",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "CarPhotos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_CarPhotos_Merchants_MerchantId",
                table: "CarPhotos",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_Merchants_MerchantId",
                table: "Rewards",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarPhotos_Merchants_MerchantId",
                table: "CarPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_Merchants_MerchantId",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "CarPhotos");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AppliedOfferId",
                table: "WashHistories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Avatar",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RewardQRCode",
                table: "Rewards",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MerchantId",
                table: "Rewards",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "MerchantComment",
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

            migrationBuilder.AlterColumn<string>(
                name: "RewardDescription",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "غسلة مجانية",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateWelcome",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "مرحباً بك في مغسلتنا",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateRewardClose",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "أنت قريب من الحصول على المكافأة",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NotificationTemplateRemaining",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "باقي لك {0} غسلات للحصول على المكافأة",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomRewardMessage",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "مبروك! لقد حصلت على مكافأة",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomBusinessTagline",
                table: "MerchantSettings",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "أفضل غسيل سيارات",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "QRCodeImageUrl",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PlanChangeRequest",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomLogo",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RewardQRCode",
                table: "LoyaltyCards",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PlateNumber",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "CustomerSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonthResetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyWashesIncluded = table.Column<int>(type: "int", nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RenewsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscribedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WashesUsedThisMonth = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSubscriptions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerSubscriptions_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MerchantServices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MonthlyWashesIncluded = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptionPlanId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantServices_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MinimumWashes = table.Column<int>(type: "int", nullable: true),
                    SendNotification = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimesUsed = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: true),
                    UsagePerCustomer = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offers_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WashHistoryServices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServiceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WashHistoryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WashHistoryServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WashHistoryServices_MerchantServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "MerchantServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WashHistoryServices_WashHistories_WashHistoryId",
                        column: x => x.WashHistoryId,
                        principalTable: "WashHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfferUsages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OfferId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WashHistoryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscountApplied = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferUsages_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferUsages_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfferUsages_WashHistories_WashHistoryId",
                        column: x => x.WashHistoryId,
                        principalTable: "WashHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Phone", "Role", "UpdatedAt" },
                values: new object[] { "a3c83ebc-6933-4ac6-9987-fda6f01e62a8", null, new DateTime(2026, 1, 7, 13, 42, 57, 29, DateTimeKind.Utc).AddTicks(7972), "mohamed@test.com", true, "System Admin", "$2a$13$Q5./HtXq/16oj2gRr6nKAerdp0x88NogjBvFBBEB2cZidC9TRDldi", "01000000000", "superadmin", new DateTime(2026, 1, 7, 13, 42, 57, 29, DateTimeKind.Utc).AddTicks(8220) });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSubscriptions_CustomerId",
                table: "CustomerSubscriptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSubscriptions_MerchantId",
                table: "CustomerSubscriptions",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantServices_MerchantId",
                table: "MerchantServices",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_MerchantId",
                table: "Offers",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferUsages_CustomerId",
                table: "OfferUsages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferUsages_OfferId",
                table: "OfferUsages",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferUsages_WashHistoryId",
                table: "OfferUsages",
                column: "WashHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CustomerId",
                table: "WalletTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_WashHistoryServices_ServiceId",
                table: "WashHistoryServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_WashHistoryServices_WashHistoryId",
                table: "WashHistoryServices",
                column: "WashHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarPhotos_Merchants_MerchantId",
                table: "CarPhotos",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
    }
}
