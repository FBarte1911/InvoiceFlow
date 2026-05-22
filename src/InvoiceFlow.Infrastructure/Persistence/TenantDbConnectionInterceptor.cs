using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace InvoiceFlow.Infrastructure.Persistence;

public sealed class TenantDbConnectionInterceptor(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        => SetTenantId(connection);

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
        => await SetTenantIdAsync(connection, cancellationToken);

    private void SetTenantId(DbConnection connection)
    {
        var tenantId = ResolveTenantId();
        if (tenantId is null) return;

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT set_config('app.current_tenant_id', @t, false)";
        AddParam(cmd, "@t", tenantId);
        cmd.ExecuteNonQuery();
    }

    private async Task SetTenantIdAsync(DbConnection connection, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        if (tenantId is null) return;

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT set_config('app.current_tenant_id', @t, false)";
        AddParam(cmd, "@t", tenantId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private string? ResolveTenantId() =>
        httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value
        ?? configuration["DevTenant:Id"];

    private static void AddParam(DbCommand cmd, string name, string value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
