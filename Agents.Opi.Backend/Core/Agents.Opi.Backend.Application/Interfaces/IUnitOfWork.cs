namespace Agents.Opi.Backend.Application.Interfaces;

public interface IUnitOfWork
{
    IAgentUserRepository Users { get; }
    IConversationRepository Conversations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
