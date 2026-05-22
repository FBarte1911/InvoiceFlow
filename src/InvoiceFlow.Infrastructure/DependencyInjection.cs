using Hangfire;
using Hangfire.InMemory;
using Hangfire.PostgreSql;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Application.Notifications.Jobs;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Notifications;
using InvoiceFlow.Domain.Subscriptions;
using InvoiceFlow.Infrastructure.Email;
using InvoiceFlow.Infrastructure.MultiTenancy;
using InvoiceFlow.Infrastructure.Payments;
using InvoiceFlow.Infrastructure.Payments.MercadoPago;
using InvoiceFlow.Infrastructure.Payments.Stripe;
using InvoiceFlow.Infrastructure.Pdf;
using InvoiceFlow.Infrastructure.Persistence;
using InvoiceFlow.Infrastructure.Persistence.Repositories;
using InvoiceFlow.Infrastructure.WhatsApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace InvoiceFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        services.AddDatabase(configuration, isDevelopment);
        services.AddRepositories();
        services.AddExternalServices(configuration);
        services.AddHangfireServices(configuration, isDevelopment);
        services.AddScoped<ICurrentTenant, CurrentTenantService>();

        return services;
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        if (isDevelopment)
        {
            services.AddDbContext<InvoiceFlowDbContext>(opts =>
                opts.UseInMemoryDatabase("InvoiceFlowDev")
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        }
        else
        {
            services.AddScoped<TenantDbConnectionInterceptor>();
            services.AddDbContext<InvoiceFlowDbContext>((sp, opts) =>
                opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.MigrationsAssembly(typeof(InvoiceFlowDbContext).Assembly.FullName))
                    .AddInterceptors(sp.GetRequiredService<TenantDbConnectionInterceptor>()));
        }

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<InvoiceFlowDbContext>());
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentReminderRepository, PaymentReminderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
    }

    private static void AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ResendEmailOptions>(configuration.GetSection("Resend"));
        services.AddResend(opts => opts.ApiToken = configuration["Resend:ApiKey"] ?? string.Empty);
        services.AddScoped<IEmailSender, ResendEmailSender>();

        services.Configure<TwilioWhatsAppOptions>(configuration.GetSection("Twilio"));
        services.AddScoped<IWhatsAppSender, TwilioWhatsAppSender>();

        services.Configure<StripeOptions>(configuration.GetSection("Stripe"));
        services.AddHttpClient("MercadoPago");
        services.AddScoped<StripePaymentGateway>();
        services.AddScoped<MercadoPagoPaymentGateway>();
        services.AddScoped<IPaymentGatewayDispatcher, PaymentGatewayDispatcher>();

        services.AddScoped<IPdfGenerator, QuestPdfInvoiceGenerator>();
        services.AddScoped<IReceiptGenerator, QuestPdfReceiptGenerator>();
        services.AddScoped<ICreditNotePdfGenerator, QuestPdfCreditNoteGenerator>();
        services.AddScoped<MercadoPagoWebhookHandler>();
    }

    private static void AddHangfireServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        if (isDevelopment)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseInMemoryStorage());
        }
        else
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));
        }

        services.AddHangfireServer();
        services.AddScoped<SendPaymentReminderJob>();
    }
}
