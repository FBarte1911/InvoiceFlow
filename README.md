# InvoiceFlow

SaaS de facturación y cobro para freelancers y pequeñas agencias de América Latina. Crea facturas profesionales, envíalas por email o WhatsApp, automatiza recordatorios de pago y lleva el control financiero en múltiples monedas.

## Stack

| Capa | Tecnología |
|------|-----------|
| Backend | C# / .NET 8 |
| Frontend | Blazor Server |
| Base de datos | PostgreSQL (Supabase) |
| ORM | Entity Framework Core 8 |
| Autenticación | Auth0 + ASP.NET Identity |
| Multi-tenancy | Finbuckle.MultiTenant |
| CQRS | MediatR |
| Validaciones | FluentValidation |
| PDF | QuestPDF |
| Email | Resend |
| WhatsApp | Twilio |
| Background jobs | Hangfire |
| Pagos | Stripe + MercadoPago |
| Logging | Serilog |
| Hosting | Railway (São Paulo) |

## Arquitectura

Monolito modular con Clean Architecture.

```
InvoiceFlow.sln
├── src/
│   ├── InvoiceFlow.Domain/         # Entidades, value objects, reglas de negocio
│   ├── InvoiceFlow.Application/    # Use cases, DTOs, handlers MediatR
│   ├── InvoiceFlow.Infrastructure/ # DB, email, PDF, pagos, WhatsApp
│   └── InvoiceFlow.Web/            # Blazor Server UI
└── tests/
    ├── InvoiceFlow.Domain.Tests/
    └── InvoiceFlow.Application.Tests/
```

**Módulos del dominio:**
- `Invoicing` — Invoice, InvoiceItem, Payment, CreditNote
- `Clients` — Client
- `Subscriptions` — Subscription, SubscriptionTier, UsageLimits (freemium)
- `Notifications` — PaymentReminder

La multi-tenancy es shared database / shared schema con discriminador por `TenantId`, inyectado automáticamente por Finbuckle en cada query de EF Core.

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (para PostgreSQL local)
- Cuenta en Auth0, Resend, Twilio, Stripe (opcionales para desarrollo)

## Levantar el entorno local

```bash
# 1. Iniciar la base de datos
docker compose up -d

# 2. Restaurar dependencias
dotnet restore

# 3. Aplicar migraciones
dotnet ef database update --project src/InvoiceFlow.Infrastructure --startup-project src/InvoiceFlow.Web

# 4. Correr la aplicación
dotnet run --project src/InvoiceFlow.Web
```

La app queda disponible en `https://localhost:5001`.

## Variables de entorno

Crear `src/InvoiceFlow.Web/appsettings.Development.json` con:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=invoiceflow;Username=postgres;Password=postgres"
  },
  "Auth0": {
    "Domain": "YOUR_AUTH0_DOMAIN",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET"
  },
  "Resend": {
    "ApiKey": "YOUR_RESEND_KEY"
  },
  "Twilio": {
    "AccountSid": "YOUR_TWILIO_SID",
    "AuthToken": "YOUR_TWILIO_TOKEN",
    "WhatsAppFrom": "whatsapp:+14155238886"
  },
  "Stripe": {
    "SecretKey": "YOUR_STRIPE_SECRET"
  }
}
```

## Tests

```bash
dotnet test
```

## Modelo de suscripción

| Tier | Precio | Límites |
|------|--------|---------|
| Starter | Gratis | 3 clientes, 5 facturas/mes, 1 moneda |
| Pro | $9/mes · $79/año | Ilimitado, multi-moneda, WhatsApp, reportes |
| Team | $24/mes · $199/año | Todo Pro + 5 usuarios, MercadoPago |

Trial de 14 días sin tarjeta de crédito. El paywall aparece al alcanzar el límite, no en el onboarding.
