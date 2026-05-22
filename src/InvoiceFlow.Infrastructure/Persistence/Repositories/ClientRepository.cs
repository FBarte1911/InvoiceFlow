using InvoiceFlow.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence.Repositories;

public sealed class ClientRepository(InvoiceFlowDbContext dbContext) : IClientRepository
{
    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Clients.FirstOrDefaultAsync(c => c.Id == id && c.IsActive, cancellationToken);

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await dbContext.Clients.FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant() && c.IsActive, cancellationToken);

    public async Task<IReadOnlyList<Client>> ListActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        await dbContext.Clients
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Clients.CountAsync(c => c.IsActive, cancellationToken);

    public async Task AddAsync(Client client, CancellationToken cancellationToken = default) =>
        await dbContext.Clients.AddAsync(client, cancellationToken);

    public Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        dbContext.Clients.Update(client);
        return Task.CompletedTask;
    }
}
