using Agents.Opi.Backend.Domain.Enums;
using Agents.Opi.Backend.Domain.Resources;
using Agents.Opi.Backend.Domain.Services;

namespace Agents.Opi.Backend.Domain.Entities;

public sealed class ConversationMessage
{
    private ConversationMessage()
    {
    }

    public ConversationMessage(
        Guid conversationId,
        AgentType agentType,
        AgentPhase? phase,
        ConversationMessageRole role,
        ConversationMessageType type,
        string content)
    {
        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException(DomainMessages.RequiredValue, nameof(conversationId));
        }

        if (phase is { } phaseValue)
        {
            AgentPhasePolicy.EnsureBelongsTo(agentType, phaseValue);
        }

        Id = Guid.NewGuid();
        ConversationId = conversationId;
        AgentType = agentType;
        Phase = phase;
        Role = role;
        Type = type;
        Content = NormalizeRequired(content, nameof(content));
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public AgentType AgentType { get; private set; }
    public AgentPhase? Phase { get; private set; }
    public ConversationMessageRole Role { get; private set; }
    public ConversationMessageType Type { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(DomainMessages.RequiredValue, parameterName);
        }

        return value.Trim();
    }
}
