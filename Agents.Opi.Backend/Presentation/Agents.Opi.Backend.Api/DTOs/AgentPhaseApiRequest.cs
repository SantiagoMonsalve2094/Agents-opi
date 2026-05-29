namespace Agents.Opi.Backend.Api.DTOs;

public sealed record AgentPhaseApiRequest(
    string AgentType,
    string Phase,
    string Input,
    string? PreviousOutput,
    Guid? ConversationId);
