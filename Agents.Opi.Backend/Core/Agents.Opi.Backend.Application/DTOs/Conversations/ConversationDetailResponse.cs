using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.DTOs.Conversations;

public sealed record ConversationDetailResponse(
    Guid Id,
    AgentType AgentType,
    string Title,
    string InitialInput,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ConversationPhaseOutputResponse> PhaseOutputs,
    IReadOnlyList<ConversationMessageResponse> Messages);
