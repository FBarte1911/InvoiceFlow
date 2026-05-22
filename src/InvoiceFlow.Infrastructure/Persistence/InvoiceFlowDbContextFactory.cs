using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InvoiceFlow.Infrastructure.Persistence;

public sealed class InvoiceFlowDbContextFactory : IDesignTimeDbContextFactory<InvoiceFlowDbContext>
{
    public InvoiceFlowDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "InvoiceFlow.Web"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=invoiceflow;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<InvoiceFlowDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(InvoiceFlowDbContext).Assembly.FullName));

        return new InvoiceFlowDbContext(optionsBuilder.Options, null!, config);
    }
}
