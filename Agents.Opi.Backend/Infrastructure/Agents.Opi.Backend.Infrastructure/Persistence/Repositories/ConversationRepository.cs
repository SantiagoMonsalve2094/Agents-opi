using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Domain.Entities;
using Agents.Opi.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Agents.Opi.Backend.Infrastructure.Persistence.Repositories;

public sealed class ConversationRepository(AgentDbContext dbContext) : IConversationRepository
{
    public async Task<IReadOnlyList<Conversation>> GetByUserAsync(
        Guid userId,
        AgentType agentType,
        CancellationToken cancellationToken)
    {
        return await dbContext.Conversations
            .AsNoTracking()
            .Where(conversation => conversation.UserId == userId && conversation.AgentType == agentType && !conversation.IsDeleted)
            .OrderByDescending(conversation => conversation.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Conversations
            .Include(conversation => conversation.PhaseOutputs)
            .Include(conversation => conversation.Messages)
            .FirstOrDefaultAsync(conversation => conversation.Id == id, cancellationToken);

    public void Add(Conversation conversation)
    {
        dbContext.Conversations.Add(conversation);
    }

    public void AddPhaseOutput(ConversationPhaseOutput phaseOutput)
    {
        dbContext.ConversationPhaseOutputs.Add(phaseOutput);
    }
}
