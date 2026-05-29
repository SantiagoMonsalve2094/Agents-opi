namespace Agents.Opi.Backend.Api.DTOs;

public sealed record AgentPhaseFeedbackApiRequest(
    string AgentType,
    string Phase,
    Guid ConversationId,
    string Feedback);
