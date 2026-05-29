using System.Runtime.CompilerServices;
using System.Text;
using Agents.Opi.Backend.Application.DTOs.Agent;
using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Domain.Entities;
using Agents.Opi.Backend.Domain.Enums;
using Agents.Opi.Backend.Domain.Services;
using FluentValidation;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Agent.Commands.StreamAgentPhase;

public sealed class StreamAgentPhaseHandler(
    IAgentGenerationPort generationPort,
    IAgentStrategyResolver strategyResolver,
    IUnitOfWork unitOfWork,
    IValidator<StreamAgentPhaseCommand> validator)
    : IRequestHandler<StreamAgentPhaseCommand, AgentPhaseStreamResult>
{
    public async Task<AgentPhaseStreamResult> Handle(StreamAgentPhaseCommand command, CancellationToken cancellationToken)
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
        var stream = PersistCompletedOutputAsync(command, conversation, prompt, cancellationToken);

        return new AgentPhaseStreamResult(conversation.Id, stream);
    }

    private async IAsyncEnumerable<string> PersistCompletedOutputAsync(
        StreamAgentPhaseCommand command,
        Conversation conversation,
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var output = new StringBuilder();
        var request = new GenerativeAiRequest(
            command.AgentType,
            prompt,
            AgentPhasePolicy.RequiresIstqbKnowledge(command.AgentType, command.Phase));

        await foreach (var chunk in generationPort.GenerateStreamAsync(request, cancellationToken))
        {
            output.Append(chunk);
            yield return chunk;
        }

        if (output.Length == 0)
        {
            yield break;
        }

        var phaseOutput = conversation.UpsertPhaseOutput(
            command.Phase,
            command.Input,
            command.PreviousOutput,
            output.ToString(),
            out var created);

        if (created)
        {
            unitOfWork.Conversations.AddPhaseOutput(phaseOutput);
        }

        conversation.AddMessage(ConversationMessageRole.Agent, ConversationMessageType.PhaseOutput, command.Phase, output.ToString());

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }
}
