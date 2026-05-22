using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Shared;
using InvoiceFlow.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InvoiceFlow.Infrastructure.Persistence;

public sealed class DevDataSeeder(InvoiceFlowDbContext context, IConfiguration configuration)
{
    private readonly string _tenantId = configuration["DevTenant:Id"] ?? "dev-tenant-local";

    public async Task SeedAsync()
    {
        // Run only once per in-memory lifetime
        if (await context.Clients.IgnoreQueryFilters().AnyAsync()) return;

        // ── Subscription ──────────────────────────────────────────────
        var sub = Subscription.CreateTrial(_tenantId);
        context.Set<Subscription>().Add(sub);

        // ── Clients ───────────────────────────────────────────────────
        var maria = Client.Create(_tenantId, "María González", "maria@tecstudio.uy",
            Currency.USD, "+59891234567", "TecStudio UY", "211234560017");

        var carlos = Client.Create(_tenantId, "Carlos Rodríguez", "carlos@agencia.ar",
            Currency.USD, "+5491123456789", "Agencia Digital AR", "20-30456789-4");

        var ana = Client.Create(_tenantId, "Ana Silva", "ana@freelance.br",
            Currency.BRL, "+5511987654321");

        context.Clients.AddRange(maria, carlos, ana);

        // ── Invoices ──────────────────────────────────────────────────
        var today = DateOnly.FromDateTime(DateTime.Today);

        // 1. Paid this month
        var inv1 = Invoice.Create(_tenantId, "INV-202505-001", maria.Id,
            today.AddDays(-18), today.AddDays(-8), Currency.USD, "Desarrollo web — Sprint 3");
        inv1.AddItem("Desarrollo frontend React", 1, 2500m);
        inv1.AddItem("Testing & QA", 4, 150m);
        inv1.ClearDomainEvents();
        inv1.Send(SendChannel.Email); inv1.ClearDomainEvents();
        inv1.MarkAsPaid();            inv1.ClearDomainEvents();
        context.Invoices.Add(inv1);

        // 2. Paid last month
        var inv2 = Invoice.Create(_tenantId, "INV-202504-003", carlos.Id,
            today.AddDays(-45), today.AddDays(-15), Currency.USD, "Campaña digital Q2");
        inv2.AddItem("Estrategia de contenidos", 1, 1200m);
        inv2.AddItem("Gestión de redes sociales", 2, 600m);
        inv2.ClearDomainEvents();
        inv2.Send(SendChannel.Email); inv2.ClearDomainEvents();
        inv2.MarkAsPaid();            inv2.ClearDomainEvents();
        context.Invoices.Add(inv2);

        // 3. Sent (por cobrar)
        var inv3 = Invoice.Create(_tenantId, "INV-202505-002", maria.Id,
            today.AddDays(-5), today.AddDays(25), Currency.USD, "Diseño UI/UX — v2");
        inv3.AddItem("Wireframes y prototipos", 1, 1800m);
        inv3.AddItem("Guía de estilos", 1, 700m);
        inv3.ClearDomainEvents();
        inv3.Send(SendChannel.Email); inv3.ClearDomainEvents();
        context.Invoices.Add(inv3);

        // 4. Overdue
        var inv4 = Invoice.Create(_tenantId, "INV-202504-002", carlos.Id,
            today.AddDays(-40), today.AddDays(-10), Currency.USD);
        inv4.AddItem("Consultoría estratégica", 3, 400m);
        inv4.ClearDomainEvents();
        inv4.Send(SendChannel.WhatsApp); inv4.ClearDomainEvents();
        inv4.MarkAsOverdue();            inv4.ClearDomainEvents();
        context.Invoices.Add(inv4);

        // 5. Draft
        var inv5 = Invoice.Create(_tenantId, "INV-202505-003", ana.Id,
            today, today.AddDays(30), Currency.BRL, "Ilustraciones para landing page");
        inv5.AddItem("Pack 5 ilustraciones", 1, 3500m);
        inv5.ClearDomainEvents();
        context.Invoices.Add(inv5);

        await context.SaveChangesAsync();
    }
}
