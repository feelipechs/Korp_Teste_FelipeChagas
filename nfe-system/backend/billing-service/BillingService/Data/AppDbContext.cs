using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.Number).IsUnique();
            entity.HasIndex(i => i.IdempotencyKey).IsUnique();
            entity.Property(i => i.Status).HasConversion<string>();
            entity.HasMany(i => i.Items)
                .WithOne(ii => ii.Invoice)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(ii => ii.Id);
            entity.Property(ii => ii.ProductCode).IsRequired().HasMaxLength(50);
            entity.Property(ii => ii.ProductDescription).IsRequired().HasMaxLength(200);
        });
    }
}