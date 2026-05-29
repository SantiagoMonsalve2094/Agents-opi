using Agents.Opi.Backend.Application.Features.Agent.Commands.GenerateAgentPhase;
using FluentValidation;

namespace Agents.Opi.Backend.Application.Features.Agent.Commands.StreamAgentPhase;

public sealed class StreamAgentPhaseValidator : AbstractValidator<StreamAgentPhaseCommand>
{
    public StreamAgentPhaseValidator()
    {
        RuleFor(command => new GenerateAgentPhaseCommand(
                command.AgentType,
                command.Phase,
                command.Input,
                command.PreviousOutput,
                command.ConversationId,
                command.User))
            .SetValidator(new GenerateAgentPhaseValidator());
    }
}
