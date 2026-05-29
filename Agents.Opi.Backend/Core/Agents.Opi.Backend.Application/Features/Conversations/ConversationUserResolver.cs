using Agents.Opi.Backend.Application.DTOs.Security;
using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Application.Resources;
using Agents.Opi.Backend.Domain.Entities;

namespace Agents.Opi.Backend.Application.Features.Conversations;

public static class ConversationUserResolver
{
    public static async Task<AgentUser> ResolveAsync(
        IUnitOfWork unitOfWork,
        AuthenticatedUser authenticatedUser,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByExternalIdAsync(authenticatedUser.ExternalId, cancellationToken)
            ?? throw new KeyNotFoundException(ApplicationMessages.ConversationNotFound);

        user.MarkSeen(authenticatedUser.Email, authenticatedUser.DisplayName);
        return user;
    }
}
