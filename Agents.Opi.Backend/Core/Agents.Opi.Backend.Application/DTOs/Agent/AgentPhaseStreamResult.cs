namespace Agents.Opi.Backend.Application.DTOs.Agent;

public sealed record AgentPhaseStreamResult(
    Guid ConversationId,
    IAsyncEnumerable<string> Stream);
