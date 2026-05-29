using Agents.Opi.Backend.Application.Resources;
using Agents.Opi.Backend.Domain.Services;
using FluentValidation;

namespace Agents.Opi.Backend.Application.Features.Agent.Commands.GenerateAgentPhase;

public sealed class GenerateAgentPhaseValidator : AbstractValidator<GenerateAgentPhaseCommand>
{
    public GenerateAgentPhaseValidator()
    {
        RuleFor(command => command.Input)
            .NotEmpty()
            .WithMessage(ApplicationMessages.InputRequired);

        RuleFor(command => command.User)
            .NotNull()
            .WithMessage(ApplicationMessages.UserRequired);

        RuleFor(command => command.User.ExternalId)
            .NotEmpty()
            .When(command => command.User is not null)
            .WithMessage(ApplicationMessages.UserExternalIdRequired);

        RuleFor(command => command.User.Email)
            .NotEmpty()
            .EmailAddress()
            .When(command => command.User is not null)
            .WithMessage(ApplicationMessages.UserEmailRequired);

        RuleFor(command => command)
            .Must(command => AgentPhasePolicy.BelongsTo(command.AgentType, command.Phase))
            .WithMessage(ApplicationMessages.InvalidPhase);
    }
}
