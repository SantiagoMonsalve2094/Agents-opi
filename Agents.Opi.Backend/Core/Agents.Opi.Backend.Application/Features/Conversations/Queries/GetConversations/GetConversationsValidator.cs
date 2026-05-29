using Agents.Opi.Backend.Application.Resources;
using FluentValidation;

namespace Agents.Opi.Backend.Application.Features.Conversations.Queries.GetConversations;

public sealed class GetConversationsValidator : AbstractValidator<GetConversationsQuery>
{
    public GetConversationsValidator()
    {
        RuleFor(query => query.User).NotNull().WithMessage(ApplicationMessages.UserRequired);
        RuleFor(query => query.User.ExternalId)
            .NotEmpty()
            .When(query => query.User is not null)
            .WithMessage(ApplicationMessages.UserExternalIdRequired);
    }
}
