# InvoiceFlow — Contexto del Proyecto

## Qué es

SaaS de facturación y cobro para freelancers y pequeñas agencias de América Latina. Permite crear facturas profesionales, enviarlas por email o WhatsApp, automatizar recordatorios de pago y llevar el control financiero del negocio en múltiples monedas (USD, UYU, ARS, BRL).

## Problema que resuelve

La mayoría de los freelancers LATAM gestiona cobros de forma informal: facturas en Word enviadas por WhatsApp, seguimiento en Excel, recordatorios manuales. Resultado: pagos atrasados, imagen poco profesional y cero visibilidad financiera. A esto se suma la complejidad de inflación, tipos de cambio múltiples y clientes en distintas monedas.

## Solución

Una herramienta pensada para LATAM: en español, multi-moneda, con envío por WhatsApp (canal dominante en la región) y recordatorios automáticos de pago. El freelancer crea la factura en 30 segundos, la envía con un clic y el sistema hace el seguimiento hasta confirmar el pago.

## Mercado objetivo

- Freelancers tech (devs, diseñadores UX/UI, consultores)
- Creativos digitales (diseñadores gráficos, videomakers, fotógrafos)
- Pequeñas agencias de 2 a 10 personas (marketing digital, desarrollo web)
- Geografía: Uruguay, Argentina, Brasil como mercados principales

---

## Stack tecnológico

- **Lenguaje**: C# / .NET 8
- **Frontend**: Blazor Server
- **ORM**: Entity Framework Core 8
- **Base de datos**: PostgreSQL (hosteado en Supabase)
- **Autenticación**: ASP.NET Identity + Auth0
- **Multi-tenancy**: Finbuckle.MultiTenant (row-level, shared schema)
- **CQRS**: MediatR
- **Validaciones**: FluentValidation
- **Generación de PDF**: QuestPDF
- **Email**: Resend SDK
- **WhatsApp**: Twilio WhatsApp API
- **Recordatorios automáticos**: Hangfire
- **Pagos internacionales**: Stripe.net
- **Pagos LATAM**: MercadoPago SDK
- **Hosting**: Railway (región São Paulo)
- **Logging**: Serilog

---

## Arquitectura

Monolito modular con Clean Architecture. Un solo desarrollador. No microservicios.

```
InvoiceFlow.sln
├── src/
│   ├── InvoiceFlow.Domain/         # Entidades, value objects, reglas de negocio
│   ├── InvoiceFlow.Application/    # Use cases, DTOs, MediatR handlers
│   ├── InvoiceFlow.Infrastructure/ # DB, email, PDF, pagos, WhatsApp
│   └── InvoiceFlow.Web/            # Blazor Server UI + API controllers
└── tests/
    ├── InvoiceFlow.Domain.Tests/
    └── InvoiceFlow.Application.Tests/
```

**Módulos del dominio:**
- `Invoicing`: Invoice, InvoiceItem, InvoiceStatus, Money (value object)
- `Clients`: Client
- `Subscriptions`: Subscription, SubscriptionTier, UsageLimits (lógica freemium)
- `Notifications`: PaymentReminder

**Multi-tenancy**: Finbuckle inyecta TenantId automáticamente en cada query de EF Core. Estrategia: shared database, shared schema con discriminador por TenantId.

---

## Modelo de negocio

Freemium + suscripción mensual/anual. Sin tarjeta en el trial.

| Tier | Precio mensual | Precio anual | Límites |
|------|---------------|-------------|---------|
| Starter (gratuito) | $0 | — | 3 clientes, 5 facturas/mes, 1 moneda |
| Pro (Freelancer) | $9 USD | $79 USD | Ilimitado, multi-moneda, WhatsApp, reportes, logo propio |
| Team (Agencia) | $24 USD | $199 USD | Todo Pro + 5 usuarios, portal cliente, API, MercadoPago |

**Trial**: 14 días gratis, sin tarjeta de crédito.

**Paywall**: se muestra al llegar al límite (3er cliente o 5ta factura), no en onboarding.

**Ingresos adicionales**:
- Lifetime Deal en Product Hunt: $149 USD one-time (mes 6-7)
- Pack de plantillas premium: $9 USD one-time (mes 4+)
- White label para contadores: $49 USD/mes (año 2 si hay demanda)

---

## Modelo financiero (escenario base, bootstrapped)

**Supuestos**: ARPU ponderado $10.40 USD, churn 5% → 3.5% → 2.5%, conversión free→paid 4-6%, trial→paid 38%.

| Hito | Mes estimado |
|------|-------------|
| Primeras 3 ventas manuales | Mes 2 |
| Product Hunt launch | Mes 6-7 |
| $1,000 MRR | Mes 8-9 |
| Ramen profitable ($1,500/mes founder) | Mes 10 |
| $3,000 MRR | Mes 14-15 |
| Salario de mercado sostenible | Mes 16 |
| Primer hire part-time | Mes 24 |
| $10,000 MRR | Mes 30-32 |

| Año | ARR | Clientes pagos |
|-----|-----|---------------|
| Año 1 | $21,000 | 168 |
| Año 2 | $70,000 | 560 |
| Año 3 | $175,000 | 1,400 |

**Unit economics**: CAC $8-18 (orgánico), LTV $170-310, LTV/CAC 17-21x, payback < 2 meses.

**Costos COGS mes 1**: $5 USD. **Mes 12**: ~$163 USD. No requiere inversión externa.

---

## Roadmap

**Semanas 1-2 (sin codear)**
- Validación manual: publicar en grupos de freelancers LATAM preguntando cómo facturan hoy
- Resolver el problema a mano para 3 personas (generar PDF, enviarlo, hacer seguimiento)
- Cobrar $5 por factura para validar disposición a pagar
- Si 3 personas pagaron → construir MVP

**Mes 1-2 (MVP)**
- Setup: .NET 8 + Blazor Server + PostgreSQL en Railway
- CRUD de clientes y facturas con generación PDF (QuestPDF)
- Envío por email (Resend) y WhatsApp (Twilio)
- Auth con Auth0 + multi-tenancy con Finbuckle
- Stripe para cobro de suscripciones

**Mes 3-4 (Beta)**
- Beta cerrada con 20-50 freelancers
- Trial 14 días activo
- Hangfire para recordatorios automáticos
- Iterar por feedback

**Mes 6-7 (Lanzamiento público)**
- Product Hunt launch
- SEO: artículos "cómo facturar en USD desde Uruguay/Argentina"
- Lifetime Deal para generar caja
- Objetivo: $1,000 MRR

**Mes 9-12 (Escala)**
- Integrar MercadoPago
- Lanzar tier Agencia
- Ramen profitable
- Evaluar primer hire

---

## Estado actual

Proyecto en fase de idea / pre-validación. Aún no se escribió ninguna línea de código. El próximo paso es la validación manual con freelancers reales antes de comenzar el desarrollo.
