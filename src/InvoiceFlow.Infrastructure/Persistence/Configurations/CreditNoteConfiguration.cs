using InvoiceFlow.Domain.Invoicing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("CreditNotes");
        builder.HasKey(cn => cn.Id);

        builder.Property(cn => cn.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(cn => cn.Number).IsRequired().HasMaxLength(50);
        builder.Property(cn => cn.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(cn => cn.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(cn => cn.Currency).IsRequired().HasConversion<string>().HasMaxLength(10);

        builder.OwnsOne(cn => cn.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("AmountCurrency").HasConversion<string>().HasMaxLength(10);
        });

        builder.HasIndex(cn => cn.TenantId);
        builder.HasIndex(cn => cn.OriginalInvoiceId);
        builder.HasIndex(cn => cn.Number);
    }
}
