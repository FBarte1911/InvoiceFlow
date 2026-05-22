using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class PaymentReminderConfiguration : IEntityTypeConfiguration<PaymentReminder>
{
    public void Configure(EntityTypeBuilder<PaymentReminder> builder)
    {
        builder.ToTable("PaymentReminders");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TenantId).IsRequired().HasMaxLength(64);
        builder.Property(r => r.Channel).IsRequired().HasConversion<string>();
        builder.Property(r => r.Status).IsRequired().HasConversion<string>();
        builder.Property(r => r.ErrorMessage).HasMaxLength(1000);

        builder.HasIndex(r => r.TenantId);
        builder.HasIndex(r => new { r.Status, r.ScheduledAt });
        builder.HasIndex(r => r.InvoiceId);
    }
}
