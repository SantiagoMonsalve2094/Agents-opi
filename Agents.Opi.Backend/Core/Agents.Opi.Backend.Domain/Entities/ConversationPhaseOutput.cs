using Agents.Opi.Backend.Domain.Enums;
using Agents.Opi.Backend.Domain.Resources;
using Agents.Opi.Backend.Domain.Services;

namespace Agents.Opi.Backend.Domain.Entities;

public sealed class ConversationPhaseOutput
{
    private ConversationPhaseOutput()
    {
    }

    public ConversationPhaseOutput(
        Guid conversationId,
        AgentType agentType,
        AgentPhase phase,
        string input,
        string? previousOutput,
        string output)
    {
        AgentPhasePolicy.EnsureBelongsTo(agentType, phase);

        Id = Guid.NewGuid();
        ConversationId = conversationId;
        AgentType = agentType;
        Phase = phase;
        Input = NormalizeRequired(input, nameof(input));
        PreviousOutput = previousOutput ?? string.Empty;
        Output = NormalizeRequired(output, nameof(output));
        CreatedAt = DateTimeOffset.UtcNow;
        CompletedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public AgentType AgentType { get; private set; }
    public AgentPhase Phase { get; private set; }
    public string Input { get; private set; } = string.Empty;
    public string PreviousOutput { get; private set; } = string.Empty;
    public string Output { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }

    public void Update(string input, string? previousOutput, string output)
    {
        Input = NormalizeRequired(input, nameof(input));
        PreviousOutput = previousOutput ?? string.Empty;
        Output = NormalizeRequired(output, nameof(output));
        CompletedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(DomainMessages.RequiredValue, parameterName);
        }

        return value.Trim();
    }
}
