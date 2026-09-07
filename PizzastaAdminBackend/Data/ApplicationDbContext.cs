using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PizzastaAdminBackend.Models;

namespace PizzastaAdminBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<MenuItem> MenuItems => Set<MenuItem>();

        public DbSet<MenuItemVariant> MenuItemVariants
            => Set<MenuItemVariant>();

        public DbSet<Deal> Deals => Set<Deal>();

        public DbSet<DealItem> DealItems => Set<DealItem>();

        public DbSet<Review> Reviews => Set<Review>();

        public DbSet<WebsiteSettings> WebsiteSettings { get; set; }

        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // CATEGORY -> MENU ITEMS
            // One Category has many MenuItems
            // ============================================

            modelBuilder.Entity<MenuItem>()
                .HasOne(x => x.Category)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // MENU ITEM -> VARIANTS
            // One MenuItem has many Variants
            // ============================================

            modelBuilder.Entity<MenuItemVariant>()
                .HasOne(x => x.MenuItem)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================
            // DEAL -> DEAL ITEMS
            // One Deal has many DealItems
            // ============================================

            modelBuilder.Entity<DealItem>()
                .HasOne(x => x.Deal)
                .WithMany(x => x.DealItems)
                .HasForeignKey(x => x.DealId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================
            // MENU ITEM -> DEAL ITEMS
            // One MenuItem can exist in many DealItems
            // ============================================

            modelBuilder.Entity<DealItem>()
                .HasOne(x => x.MenuItem)
                .WithMany(x => x.DealItems)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================
            // DEAL ITEM -> MENU ITEM VARIANT
            // Optional variant
            // ============================================

            modelBuilder.Entity<DealItem>()
                .HasOne(x => x.MenuItemVariant)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // DECIMAL CONFIGURATION
            // ============================================

            modelBuilder.Entity<MenuItemVariant>()
                .Property(x => x.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Deal>()
                .Property(x => x.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Deal>()
                .Property(x => x.OriginalPrice)
                .HasPrecision(10, 2);

            // ============================================
            // UNIQUE INDEXES
            // ============================================

            modelBuilder.Entity<Category>()
                .HasIndex(x => x.Label)
                .IsUnique();

            modelBuilder.Entity<MenuItem>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            // ============================================
            // DISPLAY ORDER
            // ============================================

            modelBuilder.Entity<Category>()
                .Property(x => x.DisplayOrder)
                .HasDefaultValue(0);

            modelBuilder.Entity<MenuItem>()
                .Property(x => x.DisplayOrder)
                .HasDefaultValue(0);

            modelBuilder.Entity<Deal>()
                .Property(x => x.DisplayOrder)
                .HasDefaultValue(0);

            // ============================================
            // WEBSITE SETTINGS & LOCATION CONFIGURATION
            // ============================================

            modelBuilder.Entity<Location>()
                .Property(x => x.Latitude)
                .HasPrecision(10, 7);

            modelBuilder.Entity<Location>()
                .Property(x => x.Longitude)
                .HasPrecision(10, 7);
        }
    }
}
