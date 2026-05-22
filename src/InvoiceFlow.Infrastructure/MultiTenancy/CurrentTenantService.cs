using InvoiceFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace InvoiceFlow.Infrastructure.MultiTenancy;

public sealed class CurrentTenantService(
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment environment,
    IConfiguration configuration) : ICurrentTenant
{
    private string? DevFallbackId =>
        environment.IsDevelopment() ? (configuration["DevTenant:Id"] ?? "dev-tenant-local") : null;

    public string Id => httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value
        ?? DevFallbackId
        ?? throw new InvalidOperationException("Tenant context is not set. User must be authenticated.");

    public string? Name => httpContextAccessor.HttpContext?.User.FindFirst("tenant_name")?.Value
        ?? (environment.IsDevelopment() ? "Dev Tenant" : null);

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
