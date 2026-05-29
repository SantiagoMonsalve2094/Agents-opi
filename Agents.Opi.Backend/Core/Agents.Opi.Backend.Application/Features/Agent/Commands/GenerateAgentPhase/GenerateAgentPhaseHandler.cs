using Agents.Opi.Backend.Application.DTOs.Agent;
using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Domain.Enums;
using Agents.Opi.Backend.Domain.Services;
using FluentValidation;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Agent.Commands.GenerateAgentPhase;

public sealed class GenerateAgentPhaseHandler(
    IAgentGenerationPort generationPort,
    IAgentStrategyResolver strategyResolver,
    IUnitOfWork unitOfWork,
    IValidator<GenerateAgentPhaseCommand> validator)
    : IRequestHandler<GenerateAgentPhaseCommand, AgentPhaseResult>
{
    public async Task<AgentPhaseResult> Handle(GenerateAgentPhaseCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var conversation = await ConversationSession.LoadOrCreateAsync(
            unitOfWork,
            command.User,
            command.AgentType,
            command.ConversationId,
            command.Input,
            cancellationToken);

        var strategy = strategyResolver.Resolve(command.AgentType);
        var prompt = strategy.BuildPrompt(command.Phase, command.Input, command.PreviousOutput);
        var output = await generationPort.GenerateAsync(
            new GenerativeAiRequest(
                command.AgentType,
                prompt,
                AgentPhasePolicy.RequiresIstqbKnowledge(command.AgentType, command.Phase)),
            cancellationToken);

        var phaseOutput = conversation.UpsertPhaseOutput(command.Phase, command.Input, command.PreviousOutput, output, out var created);
        if (created)
        {
            unitOfWork.Conversations.AddPhaseOutput(phaseOutput);
        }

        conversation.AddMessage(ConversationMessageRole.Agent, ConversationMessageType.PhaseOutput, command.Phase, output);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new AgentPhaseResult(output, conversation.Id);
    }
}
