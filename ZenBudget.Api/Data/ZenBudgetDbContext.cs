using Microsoft.EntityFrameworkCore;
using ZenBudget.Api.Models;

namespace ZenBudget.Api.Data;

public class ZenBudgetDbContext : DbContext
{
    public ZenBudgetDbContext(DbContextOptions<ZenBudgetDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Currency).HasDefaultValue("PHP").HasMaxLength(10);
            entity.Property(u => u.Appearance).HasDefaultValue("system").HasMaxLength(20);
        });

        // ── Category ──
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.BudgetLimit).HasPrecision(18, 2);

            entity.HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Transaction ──
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.HasIndex(t => t.Timestamp);

            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
