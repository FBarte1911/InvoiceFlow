using InvoiceFlow.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(s => s.Tier).IsRequired().HasConversion<string>();
        builder.Property(s => s.StripeCustomerId).HasMaxLength(100);
        builder.Property(s => s.StripeSubscriptionId).HasMaxLength(100);
        builder.Property(s => s.DefaultTaxRate).HasPrecision(5, 2);
        builder.Property(s => s.TaxLabel).HasMaxLength(20);
        builder.Property(s => s.MercadoPagoAccessToken).HasMaxLength(200);

        builder.HasIndex(s => s.TenantId).IsUnique();
    }
}
