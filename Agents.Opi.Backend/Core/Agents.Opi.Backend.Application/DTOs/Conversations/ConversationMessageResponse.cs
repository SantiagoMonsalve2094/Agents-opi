using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.DTOs.Conversations;

public sealed record ConversationMessageResponse(
    Guid Id,
    AgentPhase? Phase,
    ConversationMessageRole Role,
    ConversationMessageType Type,
    string Content,
    DateTimeOffset CreatedAt);
