using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Domain.Entities;
using RestaurantBackend.Domain.Enums;

namespace RestaurantBackend.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).HasConversion<string>();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Relationships
            entity.HasMany(e => e.Orders)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Price).HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.Stock).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Relationships
            entity.HasMany(e => e.OrderItems)
                .WithOne(e => e.Product)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Total).HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Relationships
            entity.HasMany(e => e.OrderItems)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Price).HasPrecision(10, 2).IsRequired();
        });

        // Seed data - Admin user
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Create admin user (password: Admin123!)
        // Hardcoded hash for "Admin123!" to avoid dynamic model changes
        const string adminPasswordHash = "$2a$11$qM87vAnm.iS/vSUp9R0hgeOQyK6LzJpX0XkQYI7W5Yy2z6n1X.y6K";
        var staticDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Administrator",
                Email = "admin@restaurant.com",
                PasswordHash = adminPasswordHash,
                Role = UserRole.ADMIN,
                CreatedAt = staticDate,
                UpdatedAt = staticDate
            }
        );
    }
}
