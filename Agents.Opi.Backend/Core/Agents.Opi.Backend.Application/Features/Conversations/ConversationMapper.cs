using Agents.Opi.Backend.Application.DTOs.Conversations;
using Agents.Opi.Backend.Domain.Entities;
using Agents.Opi.Backend.Domain.Services;

namespace Agents.Opi.Backend.Application.Features.Conversations;

public static class ConversationMapper
{
    public static ConversationSummaryResponse ToSummary(Conversation conversation)
        => new(
            conversation.Id,
            conversation.AgentType,
            conversation.Title,
            conversation.CreatedAt,
            conversation.UpdatedAt);

    public static ConversationDetailResponse ToDetail(Conversation conversation)
    {
        var orderedOutputs = conversation.PhaseOutputs
            .OrderBy(output => output.CreatedAt)
            .ToList();

        return new ConversationDetailResponse(
            conversation.Id,
            conversation.AgentType,
            conversation.Title,
            conversation.InitialInput,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            orderedOutputs
                .Select(output => new ConversationPhaseOutputResponse(
                    output.Id,
                    output.Phase,
                    output.Input,
                    output.PreviousOutput,
                    output.Output,
                    output.CreatedAt,
                    output.CompletedAt,
                    IsStale(output, orderedOutputs)))
                .ToList(),
            conversation.Messages
                .OrderBy(message => message.CreatedAt)
                .Select(message => new ConversationMessageResponse(
                    message.Id,
                    message.Phase,
                    message.Role,
                    message.Type,
                    message.Content,
                    message.CreatedAt))
                .ToList());
    }

    private static bool IsStale(ConversationPhaseOutput output, IReadOnlyCollection<ConversationPhaseOutput> outputs)
        => outputs.Any(candidate =>
            candidate.CompletedAt > output.CompletedAt &&
            AgentPhasePolicy.BelongsTo(output.AgentType, candidate.Phase) &&
            candidate.Phase < output.Phase);
}
