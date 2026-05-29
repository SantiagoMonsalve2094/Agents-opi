using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agents.Opi.Backend.Infrastructure.Persistence.Repositories;

public sealed class AgentUserRepository(AgentDbContext dbContext) : IAgentUserRepository
{
    public Task<AgentUser?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
        => dbContext.AgentUsers.FirstOrDefaultAsync(user => user.ExternalId == externalId, cancellationToken);

    public void Add(AgentUser user)
    {
        dbContext.AgentUsers.Add(user);
    }
}
