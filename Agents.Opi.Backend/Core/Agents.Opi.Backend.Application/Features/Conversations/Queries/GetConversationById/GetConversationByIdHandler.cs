using Agents.Opi.Backend.Application.DTOs.Conversations;
using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Application.Resources;
using FluentValidation;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Conversations.Queries.GetConversationById;

public sealed class GetConversationByIdHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetConversationByIdQuery> validator)
    : IRequestHandler<GetConversationByIdQuery, ConversationDetailResponse>
{
    public async Task<ConversationDetailResponse> Handle(
        GetConversationByIdQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await ConversationUserResolver.ResolveAsync(unitOfWork, request.User, cancellationToken);
        var conversation = await unitOfWork.Conversations.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException(ApplicationMessages.ConversationNotFound);

        if (conversation.UserId != user.Id || conversation.AgentType != request.AgentType || conversation.IsDeleted)
        {
            throw new UnauthorizedAccessException(ApplicationMessages.ConversationForbidden);
        }

        return ConversationMapper.ToDetail(conversation);
    }
}
