using Agents.Opi.Backend.Domain.Entities;

namespace Agents.Opi.Backend.Application.Interfaces;

public interface IAgentUserRepository
{
    Task<AgentUser?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);
    void Add(AgentUser user);
}
