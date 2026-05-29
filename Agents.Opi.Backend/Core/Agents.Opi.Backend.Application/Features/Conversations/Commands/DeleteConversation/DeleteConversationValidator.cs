using Agents.Opi.Backend.Application.Resources;
using FluentValidation;

namespace Agents.Opi.Backend.Application.Features.Conversations.Commands.DeleteConversation;

public sealed class DeleteConversationValidator : AbstractValidator<DeleteConversationCommand>
{
    public DeleteConversationValidator()
    {
        RuleFor(command => command.ConversationId).NotEmpty();
        RuleFor(command => command.User).NotNull().WithMessage(ApplicationMessages.UserRequired);
        RuleFor(command => command.User.ExternalId)
            .NotEmpty()
            .When(command => command.User is not null)
            .WithMessage(ApplicationMessages.UserExternalIdRequired);
    }
}
