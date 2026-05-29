using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Infrastructure.Persistence.Repositories;

namespace Agents.Opi.Backend.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AgentDbContext _dbContext;

    public UnitOfWork(AgentDbContext dbContext)
    {
        _dbContext = dbContext;
        Users = new AgentUserRepository(dbContext);
        Conversations = new ConversationRepository(dbContext);
    }

    public IAgentUserRepository Users { get; }
    public IConversationRepository Conversations { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
