using Agents.Opi.Backend.Application.DTOs.Security;
using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Application.Resources;
using Agents.Opi.Backend.Domain.Entities;
using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.Features.Agent;

public static class ConversationSession
{
    public static async Task<Conversation> LoadOrCreateAsync(
        IUnitOfWork unitOfWork,
        AuthenticatedUser authenticatedUser,
        AgentType agentType,
        Guid? conversationId,
        string input,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByExternalIdAsync(authenticatedUser.ExternalId, cancellationToken);
        if (user is null)
        {
            user = new AgentUser(authenticatedUser.ExternalId, authenticatedUser.Email, authenticatedUser.DisplayName);
            unitOfWork.Users.Add(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            user.MarkSeen(authenticatedUser.Email, authenticatedUser.DisplayName);
        }

        if (conversationId is null || conversationId == Guid.Empty)
        {
            var created = new Conversation(user.Id, agentType, input);
            unitOfWork.Conversations.Add(created);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return created;
        }

        var conversation = await unitOfWork.Conversations.GetByIdAsync(conversationId.Value, cancellationToken)
            ?? throw new KeyNotFoundException(ApplicationMessages.ConversationNotFound);

        if (conversation.UserId != user.Id || conversation.AgentType != agentType)
        {
            throw new UnauthorizedAccessException(ApplicationMessages.ConversationForbidden);
        }

        return conversation;
    }
}
