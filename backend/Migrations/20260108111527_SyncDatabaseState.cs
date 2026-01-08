using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SyncDatabaseState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =====================================================
            // SAFE DATABASE CLEANUP - Handles partially failed previous migration
            // =====================================================

            // 1. Drop foreign keys safely (they may or may not exist)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CarPhotos_Merchants_MerchantId')
                    ALTER TABLE [CarPhotos] DROP CONSTRAINT [FK_CarPhotos_Merchants_MerchantId];
                
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Customers_Merchants_MerchantId')
                    ALTER TABLE [Customers] DROP CONSTRAINT [FK_Customers_Merchants_MerchantId];
                
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Rewards_Merchants_MerchantId')
                    ALTER TABLE [Rewards] DROP CONSTRAINT [FK_Rewards_Merchants_MerchantId];
            ");

            // 2. Drop unused tables if they exist
            migrationBuilder.Sql(@"
                IF OBJECT_ID('CustomerSubscriptions', 'U') IS NOT NULL DROP TABLE [CustomerSubscriptions];
                IF OBJECT_ID('OfferUsages', 'U') IS NOT NULL DROP TABLE [OfferUsages];
                IF OBJECT_ID('WalletTransactions', 'U') IS NOT NULL DROP TABLE [WalletTransactions];
                IF OBJECT_ID('WashHistoryServices', 'U') IS NOT NULL DROP TABLE [WashHistoryServices];
                IF OBJECT_ID('Offers', 'U') IS NOT NULL DROP TABLE [Offers];
                IF OBJECT_ID('MerchantServices', 'U') IS NOT NULL DROP TABLE [MerchantServices];
            ");

            // 3. Add missing columns to Merchants (Logo) - safely
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Merchants]') AND name = 'Logo')
                    ALTER TABLE [Merchants] ADD [Logo] nvarchar(max) NOT NULL DEFAULT N'';
            ");

            // 4. Add WalletBalance back to Customers if it was dropped
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Customers]') AND name = 'WalletBalance')
                    ALTER TABLE [Customers] ADD [WalletBalance] decimal(18,2) NOT NULL DEFAULT 0;
            ");

            // 5. Add TotalRevenue back to Merchants if it was dropped
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Merchants]') AND name = 'TotalRevenue')
                    ALTER TABLE [Merchants] ADD [TotalRevenue] decimal(18,2) NOT NULL DEFAULT 0;
            ");

            // 6. Add Value back to Rewards if it was dropped
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rewards]') AND name = 'Value')
                    ALTER TABLE [Rewards] ADD [Value] decimal(18,2) NOT NULL DEFAULT 0;
            ");

            // 7. Add UploadedAt to CarPhotos
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CarPhotos]') AND name = 'UploadedAt')
                    ALTER TABLE [CarPhotos] ADD [UploadedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
            ");

            // 8. Add RewardDescriptionAr to MerchantSettings if missing
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MerchantSettings]') AND name = 'RewardDescriptionAr')
                    ALTER TABLE [MerchantSettings] ADD [RewardDescriptionAr] nvarchar(max) NOT NULL DEFAULT N'';
            ");

            // 9. Ensure nullable strings have defaults for non-null columns
            migrationBuilder.Sql(@"
                UPDATE [WashHistories] SET [Comments] = '' WHERE [Comments] IS NULL;
                UPDATE [WashHistories] SET [AppliedOfferId] = '' WHERE [AppliedOfferId] IS NULL;
                UPDATE [Users] SET [Avatar] = '' WHERE [Avatar] IS NULL;
                UPDATE [Rewards] SET [RewardQRCode] = '' WHERE [RewardQRCode] IS NULL;
                UPDATE [Rewards] SET [MerchantComment] = '' WHERE [MerchantComment] IS NULL;
                UPDATE [Rewards] SET [ClaimedByMerchantId] = '' WHERE [ClaimedByMerchantId] IS NULL;
                UPDATE [MerchantSettings] SET [RewardDescription] = '' WHERE [RewardDescription] IS NULL;
                UPDATE [MerchantSettings] SET [NotificationTemplateWelcome] = '' WHERE [NotificationTemplateWelcome] IS NULL;
                UPDATE [MerchantSettings] SET [NotificationTemplateRewardClose] = '' WHERE [NotificationTemplateRewardClose] IS NULL;
                UPDATE [MerchantSettings] SET [NotificationTemplateRemaining] = '' WHERE [NotificationTemplateRemaining] IS NULL;
                UPDATE [MerchantSettings] SET [CustomRewardMessage] = '' WHERE [CustomRewardMessage] IS NULL;
                UPDATE [MerchantSettings] SET [CustomBusinessTagline] = '' WHERE [CustomBusinessTagline] IS NULL;
                UPDATE [Merchants] SET [QRCodeImageUrl] = '' WHERE [QRCodeImageUrl] IS NULL;
                UPDATE [Merchants] SET [PlanChangeRequest] = '' WHERE [PlanChangeRequest] IS NULL;
                UPDATE [Merchants] SET [RegistrationCode] = '' WHERE [RegistrationCode] IS NULL;
                UPDATE [Merchants] SET [CustomLogo] = '' WHERE [CustomLogo] IS NULL;
                UPDATE [LoyaltyCards] SET [RewardQRCode] = '' WHERE [RewardQRCode] IS NULL;
                UPDATE [Customers] SET [PlateNumber] = '' WHERE [PlateNumber] IS NULL;
            ");

            // 10. Update Rewards MerchantId for null values
            migrationBuilder.Sql(@"
                UPDATE r SET r.MerchantId = c.MerchantId
                FROM [Rewards] r
                INNER JOIN [Customers] c ON r.CustomerId = c.Id
                WHERE r.MerchantId IS NULL OR r.MerchantId = '';
            ");

            // 11. Re-add foreign keys
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CarPhotos_Merchants_MerchantId')
                    ALTER TABLE [CarPhotos] ADD CONSTRAINT [FK_CarPhotos_Merchants_MerchantId]
                    FOREIGN KEY ([MerchantId]) REFERENCES [Merchants]([Id]);
            ");
            
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Customers_Merchants_MerchantId')
                    ALTER TABLE [Customers] ADD CONSTRAINT [FK_Customers_Merchants_MerchantId]
                    FOREIGN KEY ([MerchantId]) REFERENCES [Merchants]([Id]) ON DELETE NO ACTION;
            ");
            
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Rewards_Merchants_MerchantId')
                    ALTER TABLE [Rewards] ADD CONSTRAINT [FK_Rewards_Merchants_MerchantId]
                    FOREIGN KEY ([MerchantId]) REFERENCES [Merchants]([Id]) ON DELETE NO ACTION;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
