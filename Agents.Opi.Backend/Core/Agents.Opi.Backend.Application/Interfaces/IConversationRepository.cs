using Agents.Opi.Backend.Domain.Entities;
using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.Interfaces;

public interface IConversationRepository
{
    Task<IReadOnlyList<Conversation>> GetByUserAsync(Guid userId, AgentType agentType, CancellationToken cancellationToken);
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(Conversation conversation);
    void AddPhaseOutput(ConversationPhaseOutput phaseOutput);
}
