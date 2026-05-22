using InvoiceFlow.Domain.Invoicing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(i => i.Number).IsRequired().HasMaxLength(50);
        builder.Property(i => i.ClientId).IsRequired();
        builder.Property(i => i.Status).IsRequired().HasConversion<string>();
        builder.Property(i => i.Currency).IsRequired().HasConversion<string>();
        builder.Property(i => i.Notes).HasMaxLength(2000);
        builder.Property(i => i.StripePaymentLink).HasMaxLength(500);

        builder.OwnsOne(i => i.Subtotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("SubtotalAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("SubtotalCurrency").HasConversion<string>().HasMaxLength(10);
        });

        builder.OwnsOne(i => i.Total, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("TotalCurrency").HasConversion<string>().HasMaxLength(10);
        });

        builder.OwnsOne(i => i.DiscountAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("DiscountAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("DiscountCurrency").HasConversion<string>().HasMaxLength(10);
        });

        builder.Property(i => i.DiscountType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.DiscountValue).HasPrecision(18, 2);
        builder.Property(i => i.ReceiptSentAt);

        builder.Property(i => i.TaxRate).HasPrecision(5, 2);
        builder.OwnsOne(i => i.TaxAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TaxAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("TaxCurrency").HasConversion<string>().HasMaxLength(10);
        });

        builder.OwnsOne(i => i.AmountPaid, money =>
        {
            money.Property(m => m.Amount).HasColumnName("AmountPaidAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("AmountPaidCurrency").HasConversion<string>().HasMaxLength(10);
        });

        builder.Ignore(i => i.BalanceDue);
        builder.Property(i => i.MercadoPagoPaymentLink).HasMaxLength(500);

        builder.HasMany(i => i.Items).WithOne().HasForeignKey(item => item.InvoiceId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.TenantId);
        builder.HasIndex(i => i.Number);
        builder.HasIndex(i => new { i.ClientId, i.Status });
    }
}
