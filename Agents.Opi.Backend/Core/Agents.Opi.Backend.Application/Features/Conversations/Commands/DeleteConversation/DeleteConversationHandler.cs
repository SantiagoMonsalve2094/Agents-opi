using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Application.Resources;
using FluentValidation;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Conversations.Commands.DeleteConversation;

public sealed class DeleteConversationHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteConversationCommand> validator)
    : IRequestHandler<DeleteConversationCommand>
{
    public async Task Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await ConversationUserResolver.ResolveAsync(unitOfWork, request.User, cancellationToken);
        var conversation = await unitOfWork.Conversations.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException(ApplicationMessages.ConversationNotFound);

        if (conversation.UserId != user.Id || conversation.AgentType != request.AgentType)
        {
            throw new UnauthorizedAccessException(ApplicationMessages.ConversationForbidden);
        }

        conversation.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
