using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.DTOs.Conversations;

public sealed record ConversationSummaryResponse(
    Guid Id,
    AgentType AgentType,
    string Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
