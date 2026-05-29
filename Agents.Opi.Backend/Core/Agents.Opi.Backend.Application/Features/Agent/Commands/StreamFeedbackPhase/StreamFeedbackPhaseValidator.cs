using Agents.Opi.Backend.Application.Resources;
using Agents.Opi.Backend.Domain.Services;
using FluentValidation;

namespace Agents.Opi.Backend.Application.Features.Agent.Commands.StreamFeedbackPhase;

public sealed class StreamFeedbackPhaseValidator : AbstractValidator<StreamFeedbackPhaseCommand>
{
    public StreamFeedbackPhaseValidator()
    {
        RuleFor(command => command.AgentType)
            .IsInEnum()
            .WithMessage(ApplicationMessages.InvalidAgentType);

        RuleFor(command => command.Phase)
            .IsInEnum()
            .Must((command, phase) => AgentPhasePolicy.BelongsTo(command.AgentType, phase))
            .WithMessage(ApplicationMessages.InvalidPhase);

        RuleFor(command => command.ConversationId)
            .NotEmpty()
            .WithMessage(ApplicationMessages.ConversationRequired);

        RuleFor(command => command.Feedback)
            .NotEmpty()
            .WithMessage(ApplicationMessages.FeedbackRequired);

        RuleFor(command => command.User)
            .NotNull()
            .WithMessage(ApplicationMessages.UserRequired);

        RuleFor(command => command.User.ExternalId)
            .NotEmpty()
            .When(command => command.User is not null)
            .WithMessage(ApplicationMessages.UserExternalIdRequired);
    }
}
