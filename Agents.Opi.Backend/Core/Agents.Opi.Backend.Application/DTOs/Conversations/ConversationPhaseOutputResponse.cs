using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.DTOs.Conversations;

public sealed record ConversationPhaseOutputResponse(
    Guid Id,
    AgentPhase Phase,
    string Input,
    string PreviousOutput,
    string Output,
    DateTimeOffset CreatedAt,
    DateTimeOffset CompletedAt,
    bool IsStale);
