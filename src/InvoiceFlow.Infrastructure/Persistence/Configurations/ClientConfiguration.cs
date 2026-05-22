using InvoiceFlow.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Company).HasMaxLength(200);
        builder.Property(c => c.TaxId).HasMaxLength(50);
        builder.Property(c => c.PreferredCurrency).IsRequired().HasConversion<string>().HasMaxLength(10);

        builder.HasIndex(c => new { c.TenantId, c.Email });
        builder.HasIndex(c => new { c.TenantId, c.IsActive });
    }
}
