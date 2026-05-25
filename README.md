# InvoiceFlow

SaaS de facturación y cobros para freelancers y pequeñas agencias de América Latina. Crea facturas profesionales, envíalas por email o WhatsApp, automatiza recordatorios de pago y lleva el control financiero en múltiples monedas.

> Proyecto personal en desarrollo activo. Construido con Clean Architecture, CQRS y multi-tenancy para explorar patrones de diseño aplicados a un producto real.

## Funcionalidades

- **Facturación completa** — crear, editar, enviar y cancelar facturas con múltiples ítems
- **Multi-canal** — envío de facturas por email (PDF adjunto) o WhatsApp
- **Cobro online** — links de pago generados con Stripe (tarjeta) o MercadoPago (métodos locales LatAm)
- **Recordatorios automáticos** — Hangfire dispara recordatorios de pago por email/WhatsApp según vencimiento
- **Notas de crédito** — emisión contra facturas pagadas
- **Multi-moneda** — USD, UYU, BRL con value object `Money` que previene operaciones entre monedas distintas
- **Multi-tenancy** — cada cuenta tiene sus datos aislados; arquitectura shared schema con filtros por `TenantId`
- **Modelo freemium** — Starter gratis, Pro y Team con Stripe Billing

## Stack

| Capa | Tecnología |
|------|-----------|
| Backend | C# / .NET 8 |
| Frontend | Blazor Server |
| Base de datos | PostgreSQL (Supabase) |
| ORM | Entity Framework Core 8 |
| Autenticación | Auth0 |
| Multi-tenancy | Shared schema con query filters en EF Core |
| CQRS | MediatR + FluentValidation |
| PDF | QuestPDF |
| Email | Resend |
| WhatsApp | Twilio |
| Background jobs | Hangfire |
| Pagos | Stripe + MercadoPago |
| Logging | Serilog |
| Hosting | Railway (São Paulo) |

## Arquitectura

Monolito modular con Clean Architecture. Las dependencias apuntan hacia adentro: Domain no conoce a nadie, Application solo conoce a Domain, Infrastructure implementa las interfaces que Application define.

```
InvoiceFlow.sln
├── src/
│   ├── InvoiceFlow.Domain/         # Entidades, value objects, reglas de negocio, domain events
│   ├── InvoiceFlow.Application/    # Use cases (commands/queries), interfaces, pipeline behaviors
│   ├── InvoiceFlow.Infrastructure/ # EF Core, email, PDF, pagos, WhatsApp, multi-tenancy
│   └── InvoiceFlow.Web/            # Blazor Server, endpoints HTTP, DI composition root
└── tests/
    ├── InvoiceFlow.Domain.Tests/        # Tests del modelo de dominio
    └── InvoiceFlow.Application.Tests/  # Tests de use cases
```

### Decisiones de diseño

**CQRS con MediatR**
Cada operación es un `Command` o `Query` con su propio handler. El pipeline de MediatR aplica validaciones con FluentValidation y logging automáticamente antes de que el handler ejecute, sin código repetido.

**Dependency inversion para servicios externos**
Application define interfaces (`IEmailSender`, `IWhatsAppSender`, `IPaymentGateway`). Infrastructure las implementa con los SDKs de Resend, Twilio y Stripe/MercadoPago. Los use cases no conocen los servicios externos concretos — si se cambia de proveedor, solo cambia la implementación en Infrastructure.

**Multi-tenancy shared schema**
Todos los tenants comparten las mismas tablas. EF Core aplica un query filter global por `TenantId` en cada entidad, inyectado desde el JWT del usuario autenticado. El costo de infraestructura es mínimo para un SaaS early-stage.

**Dispatcher de pagos**
`PaymentGatewayDispatcher` elige entre Stripe y MercadoPago en runtime según si el tenant configuró su token de MercadoPago. Stripe es el default para tarjetas internacionales; MercadoPago habilita métodos locales de LatAm.

**Domain events**
Las entidades emiten eventos de dominio (`InvoiceCreatedEvent`, etc.) que se procesan desacoplados del comando que los originó.

### Módulos del dominio

| Módulo | Entidades |
|--------|-----------|
| `Invoicing` | Invoice, InvoiceItem, Payment, CreditNote |
| `Clients` | Client |
| `Subscriptions` | Subscription, SubscriptionTier, UsageLimits |
| `Notifications` | PaymentReminder |
| `Shared` | Money (value object), Currency, Entity base |

## Correr el proyecto localmente

Solo necesitás [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). No se necesita Docker ni cuentas en servicios externos.

```bash
git clone https://github.com/FBarte1911/InvoiceFlow.git
cd InvoiceFlow
dotnet run --project src/InvoiceFlow.Web
```

Abre `http://localhost:5281`.

En modo `Development` (activo por defecto):
- Base de datos en memoria — sin PostgreSQL ni Docker
- Login automático sin Auth0 — se crea una sesión de dev al entrar a cualquier ruta protegida
- Seeder carga datos de prueba: 3 clientes, 5 facturas en distintos estados (pagada, enviada, vencida, borrador) y multi-moneda
- Hangfire en memoria

## Tests

```bash
dotnet test
```

Los tests de dominio cubren las reglas de negocio centrales: ciclo de vida de la factura (Draft → Sent → Paid / Cancelled), cálculo de totales, invariantes del value object `Money` (monedas incompatibles, montos negativos).

## Modelo de suscripción

| Tier | Precio | Límites |
|------|--------|---------|
| Starter | Gratis | 3 clientes, 5 facturas/mes, 1 moneda |
| Pro | $9/mes · $79/año | Ilimitado, multi-moneda, WhatsApp, reportes |
| Team | $24/mes · $199/año | Todo Pro + 5 usuarios, MercadoPago |

Trial de 14 días sin tarjeta de crédito. El paywall aparece al alcanzar el límite, no en el onboarding.

## Configuración para producción

Variables de entorno en Railway (o `appsettings.Production.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=invoiceflow;Username=postgres;Password=..."
  },
  "Auth0": {
    "Domain": "YOUR_AUTH0_DOMAIN",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET"
  },
  "Resend": { "ApiKey": "YOUR_RESEND_KEY" },
  "Twilio": {
    "AccountSid": "YOUR_TWILIO_SID",
    "AuthToken": "YOUR_TWILIO_TOKEN",
    "WhatsAppFrom": "whatsapp:+14155238886"
  },
  "Stripe": {
    "SecretKey": "YOUR_STRIPE_SECRET",
    "WebhookSecret": "YOUR_WEBHOOK_SECRET"
  }
}
```
