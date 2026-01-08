using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Users
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Merchant> Merchants { get; set; }

        // Loyalty & Rewards
        public DbSet<LoyaltyCard> LoyaltyCards { get; set; }
        public DbSet<Reward> Rewards { get; set; }

        // Transactions & History
        public DbSet<WashHistory> WashHistories { get; set; }
        public DbSet<CarPhoto> CarPhotos { get; set; }

        // Notifications & Settings
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MerchantSettings> MerchantSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Customer Configuration
            modelBuilder.Entity<Customer>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.QRCode)
                .IsUnique();

            // Merchant Configuration
            modelBuilder.Entity<Merchant>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Merchant>()
                .HasOne(m => m.User)
                .WithOne(u => u.Merchant)
                .HasForeignKey<Merchant>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // LoyaltyCard Configuration
            modelBuilder.Entity<LoyaltyCard>()
                .HasKey(lc => lc.Id);

            modelBuilder.Entity<LoyaltyCard>()
                .HasOne(lc => lc.Customer)
                .WithMany(c => c.LoyaltyCards)
                .HasForeignKey(lc => lc.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoyaltyCard>()
                .HasOne(lc => lc.Merchant)
                .WithMany(m => m.LoyaltyCards)
                .HasForeignKey(lc => lc.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);

            // WashHistory Configuration
            modelBuilder.Entity<WashHistory>()
                .HasKey(wh => wh.Id);

            modelBuilder.Entity<WashHistory>()
                .HasOne(wh => wh.Customer)
                .WithMany(c => c.WashHistories)
                .HasForeignKey(wh => wh.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WashHistory>()
                .HasOne(wh => wh.Merchant)
                .WithMany(m => m.WashHistories)
                .HasForeignKey(wh => wh.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reward Configuration
            modelBuilder.Entity<Reward>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Reward>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reward>()
                .HasOne(r => r.Merchant)
                .WithMany()
                .HasForeignKey(r => r.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification Configuration
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Customer)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // CarPhoto Configuration
            modelBuilder.Entity<CarPhoto>()
                .HasKey(cp => cp.Id);

            modelBuilder.Entity<CarPhoto>()
                .HasOne(cp => cp.Customer)
                .WithMany()
                .HasForeignKey(cp => cp.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // MerchantSettings Configuration
            modelBuilder.Entity<MerchantSettings>()
                .HasKey(ms => ms.Id);

            modelBuilder.Entity<MerchantSettings>()
                .HasOne(ms => ms.Merchant)
                .WithMany(m => m.Settings)
                .HasForeignKey(ms => ms.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
