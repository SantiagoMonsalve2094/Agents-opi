using System.Runtime.CompilerServices;
using System.Text;
using Agents.Opi.Backend.Application.DTOs.Agent;
using Agents.Opi.Backend.Application.Features.Conversations;
using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Application.Resources;
using Agents.Opi.Backend.Domain.Entities;
using Agents.Opi.Backend.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Agent.Commands.StreamFeedbackPhase;

public sealed class StreamFeedbackPhaseHandler(
    IAgentGenerationPort generationPort,
    IAgentStrategyResolver strategyResolver,
    IUnitOfWork unitOfWork,
    IValidator<StreamFeedbackPhaseCommand> validator)
    : IRequestHandler<StreamFeedbackPhaseCommand, AgentPhaseStreamResult>
{
    public async Task<AgentPhaseStreamResult> Handle(StreamFeedbackPhaseCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await ConversationUserResolver.ResolveAsync(unitOfWork, command.User, cancellationToken);
        var conversation = await unitOfWork.Conversations.GetByIdAsync(command.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException(ApplicationMessages.ConversationNotFound);

        if (conversation.UserId != user.Id || conversation.AgentType != command.AgentType || conversation.IsDeleted)
        {
            throw new UnauthorizedAccessException(ApplicationMessages.ConversationForbidden);
        }

        var currentPhaseOutput = conversation.PhaseOutputs.FirstOrDefault(output => output.Phase == command.Phase)
            ?? throw new KeyNotFoundException(ApplicationMessages.PhaseOutputNotFound);

        var strategy = strategyResolver.Resolve(command.AgentType);
        var previousOutput = BuildPreviousOutput(conversation, command.Phase);
        var prompt = strategy.BuildFeedbackPrompt(
            command.Phase,
            conversation.InitialInput,
            previousOutput,
            currentPhaseOutput.Output,
            command.Feedback);
        var stream = PersistRefinedOutputAsync(command, conversation, previousOutput, prompt, strategy.RequiresKnowledge(command.Phase), cancellationToken);

        return new AgentPhaseStreamResult(conversation.Id, stream);
    }

    private async IAsyncEnumerable<string> PersistRefinedOutputAsync(
        StreamFeedbackPhaseCommand command,
        Conversation conversation,
        string previousOutput,
        string prompt,
        bool includeKnowledgeSource,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var output = new StringBuilder();
        var request = new GenerativeAiRequest(command.AgentType, prompt, includeKnowledgeSource);

        await foreach (var chunk in generationPort.GenerateStreamAsync(request, cancellationToken))
        {
            output.Append(chunk);
            yield return chunk;
        }

        if (output.Length == 0)
        {
            yield break;
        }

        conversation.AddMessage(ConversationMessageRole.User, ConversationMessageType.Feedback, command.Phase, command.Feedback);
        var phaseOutput = conversation.UpsertPhaseOutput(
            command.Phase,
            conversation.InitialInput,
            previousOutput,
            output.ToString(),
            out var created);

        if (created)
        {
            unitOfWork.Conversations.AddPhaseOutput(phaseOutput);
        }

        conversation.AddMessage(ConversationMessageRole.Agent, ConversationMessageType.RefinedPhaseOutput, command.Phase, output.ToString());
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    private static string BuildPreviousOutput(Conversation conversation, AgentPhase phase)
        => string.Join(
            "\n\n",
            conversation.PhaseOutputs
                .Where(output => output.Phase < phase)
                .OrderBy(output => output.Phase)
                .Select(output => $"FASE ANTERIOR: {output.Phase}\n{output.Output}"));
}
