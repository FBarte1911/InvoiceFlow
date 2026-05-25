using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Hangfire;
using Hangfire.Dashboard;
using InvoiceFlow.Application;
using InvoiceFlow.Application.Invoicing.Queries.GenerateCreditNotePdf;
using InvoiceFlow.Application.Invoicing.Queries.GenerateInvoicePdf;
using InvoiceFlow.Infrastructure;
using InvoiceFlow.Infrastructure.Jobs;
using InvoiceFlow.Infrastructure.Payments.MercadoPago;
using InvoiceFlow.Infrastructure.Persistence;
using InvoiceFlow.Web.Components;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
    builder.Services.Configure<MercadoPagoOptions>(builder.Configuration.GetSection("MercadoPago"));

    if (builder.Environment.IsDevelopment())
        builder.Services.AddScoped<DevDataSeeder>();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();
        builder.Services.AddAuthorization();
    }
    else
    {
        builder.Services.AddAuth0WebAppAuthentication(opts =>
        {
            opts.Domain = builder.Configuration["Auth0:Domain"]!;
            opts.ClientId = builder.Configuration["Auth0:ClientId"]!;
            opts.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
        });
    }

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
        app.UseHttpsRedirection();
    }
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireAuthFilter()]
    });
    HangfireReminderScheduler.RegisterRecurringJobs();

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
        await seeder.SeedAsync();
    }

    if (app.Environment.IsDevelopment())
    {
        // Auto-login en desarrollo: crea una cookie con el tenant de dev para poder navegar todas las páginas [Authorize]
        app.MapGet("/Account/Login", async (HttpContext ctx, string? returnUrl) =>
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user-local"),
                new Claim(ClaimTypes.Name, "Dev User"),
                new Claim("tenant_id", "dev-tenant-local"),
                new Claim("tenant_name", "Dev Tenant"),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            var safeUrl = Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) ? returnUrl : "/";
            ctx.Response.Redirect(safeUrl);
        });

        app.MapGet("/Account/Register", async (HttpContext ctx, string? returnUrl) =>
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user-local"),
                new Claim(ClaimTypes.Name, "Dev User"),
                new Claim("tenant_id", "dev-tenant-local"),
                new Claim("tenant_name", "Dev Tenant"),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            var safeUrl = Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) ? returnUrl : "/";
            ctx.Response.Redirect(safeUrl);
        });
    }
    else
    {
        app.MapGet("/Account/Login", async (HttpContext ctx, string? returnUrl) =>
        {
            var props = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl ?? "/")
                .Build();
            await ctx.ChallengeAsync(Auth0Constants.AuthenticationScheme, props);
        });

        app.MapGet("/Account/Register", async (HttpContext ctx, string? returnUrl) =>
        {
            var props = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl ?? "/")
                .WithParameter("screen_hint", "signup")
                .Build();
            await ctx.ChallengeAsync(Auth0Constants.AuthenticationScheme, props);
        });
    }

    app.MapGet("/Account/Logout", async (HttpContext ctx) =>
    {
        if (app.Environment.IsDevelopment())
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ctx.Response.Redirect("/");
            return;
        }
        await ctx.SignOutAsync(Auth0Constants.AuthenticationScheme, new AuthenticationProperties
        {
            RedirectUri = "/"
        });
        await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    });

    app.MapPost("/webhooks/mercadopago/{invoiceId:guid}", async (
        Guid invoiceId,
        HttpContext ctx,
        MercadoPagoWebhookHandler handler,
        CancellationToken ct) =>
    {
        var xRequestId = ctx.Request.Headers["x-request-id"].ToString();
        var xSignature = ctx.Request.Headers["x-signature"].ToString();
        var topic = ctx.Request.Query["topic"].ToString();
        var dataId = ctx.Request.Query["id"].ToString();

        var success = await handler.HandleAsync(invoiceId, dataId, xRequestId, xSignature, topic, ct);
        return success ? Results.Ok() : Results.BadRequest();
    }).AllowAnonymous();

    app.MapGet("/api/invoices/{id:guid}/pdf", async (Guid id, IMediator mediator, CancellationToken ct) =>
    {
        var bytes = await mediator.Send(new GenerateInvoicePdfQuery(id), ct);
        return Results.File(bytes, "application/pdf", $"factura-{id}.pdf");
    }).RequireAuthorization();

    app.MapGet("/api/credit-notes/{id:guid}/pdf", async (Guid id, IMediator mediator, CancellationToken ct) =>
    {
        var bytes = await mediator.Send(new GenerateCreditNotePdfQuery(id), ct);
        return Results.File(bytes, "application/pdf", $"nota-credito-{id}.pdf");
    }).RequireAuthorization();

    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}

file sealed class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) =>
        context.GetHttpContext().User.Identity?.IsAuthenticated == true;
}
