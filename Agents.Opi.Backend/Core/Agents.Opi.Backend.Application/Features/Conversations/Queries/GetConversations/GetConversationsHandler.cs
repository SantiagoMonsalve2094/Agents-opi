using Agents.Opi.Backend.Application.DTOs.Conversations;
using Agents.Opi.Backend.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Conversations.Queries.GetConversations;

public sealed class GetConversationsHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetConversationsQuery> validator)
    : IRequestHandler<GetConversationsQuery, IReadOnlyList<ConversationSummaryResponse>>
{
    public async Task<IReadOnlyList<ConversationSummaryResponse>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await ConversationUserResolver.ResolveAsync(unitOfWork, request.User, cancellationToken);
        var conversations = await unitOfWork.Conversations.GetByUserAsync(user.Id, request.AgentType, cancellationToken);

        return conversations.Select(ConversationMapper.ToSummary).ToList();
    }
}
