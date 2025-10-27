using Microsoft.EntityFrameworkCore;
using AdSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AdSystem.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ==========================
        // 🔹 DbSets (Tables)
        // ==========================
        public DbSet<Website> Websites { get; set; }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<AdImpression> AdImpressions { get; set; }
        public DbSet<AdClick> AdClicks { get; set; }

        // ==========================
        // 🔹 Model Configuration
        // ==========================
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // -----------------------------
            // Website Configuration
            // -----------------------------
            builder.Entity<Website>()
                .HasIndex(w => w.Domain)
                .IsUnique(); // Ensure no duplicate domains

            builder.Entity<Website>()
                .Property(w => w.Domain)
                .HasMaxLength(300);

            // -----------------------------
            // Ad Configuration
            // -----------------------------
            builder.Entity<Ad>()
                .Property(a => a.Title)
                .HasMaxLength(150)
                .IsRequired();

            builder.Entity<Ad>()
                .HasIndex(a => a.Status); // For fast filtering

            // Ensure cascade delete does not remove ads when advertiser is deleted
            builder.Entity<Ad>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.AdvertiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // AdImpression Configuration
            // -----------------------------
            builder.Entity<AdImpression>()
                .HasOne(i => i.Ad)
                .WithMany(a => a.Impressions)
                .HasForeignKey(i => i.AdId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AdImpression>()
                .HasIndex(i => i.AdId);

            // -----------------------------
            // AdClick Configuration
            // -----------------------------
            builder.Entity<AdClick>()
                .HasOne(c => c.Ad)
                .WithMany(a => a.Clicks)
                .HasForeignKey(c => c.AdId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AdClick>()
                .HasIndex(c => c.AdId);
            builder.Entity<Website>()
                .HasIndex(w => w.ScriptKey)
                .IsUnique();


            // -----------------------------
            // Seed data (optional)
            // -----------------------------
            // Could be used to pre-insert roles like Admin, Advertiser, Publisher
        }
    }
}
