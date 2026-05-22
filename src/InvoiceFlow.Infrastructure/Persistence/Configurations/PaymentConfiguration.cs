using InvoiceFlow.Domain.Invoicing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(p => p.InvoiceId).IsRequired();
        builder.Property(p => p.Method).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.Property(p => p.PaidAt).IsRequired();

        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(10);
        });

        builder.HasIndex(p => p.InvoiceId);
        builder.HasIndex(p => p.TenantId);
    }
}
