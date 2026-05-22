using InvoiceFlow.Domain.Invoicing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Quantity).HasPrecision(18, 4);

        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("UnitPriceCurrency").HasConversion<string>();
        });

        builder.Ignore(i => i.Total);
    }
}
