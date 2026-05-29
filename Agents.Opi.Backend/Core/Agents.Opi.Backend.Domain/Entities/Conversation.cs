using Agents.Opi.Backend.Domain.Enums;
using Agents.Opi.Backend.Domain.Resources;
using Agents.Opi.Backend.Domain.Services;
using Agents.Opi.Backend.Domain.ValueObjects;

namespace Agents.Opi.Backend.Domain.Entities;

public sealed class Conversation
{
    private readonly List<ConversationPhaseOutput> _phaseOutputs = [];
    private readonly List<ConversationMessage> _messages = [];

    private Conversation()
    {
    }

    public Conversation(Guid userId, AgentType agentType, string initialInput)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(DomainMessages.RequiredUser, nameof(userId));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        AgentType = agentType;
        InitialInput = NormalizeRequired(initialInput, nameof(initialInput));
        Title = ConversationTitle.FromInput(InitialInput).Value;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        AddMessage(ConversationMessageRole.User, ConversationMessageType.InitialInput, null, InitialInput);
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public AgentType AgentType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string InitialInput { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public IReadOnlyCollection<ConversationPhaseOutput> PhaseOutputs => _phaseOutputs;
    public IReadOnlyCollection<ConversationMessage> Messages => _messages;

    public ConversationPhaseOutput UpsertPhaseOutput(
        AgentPhase phase,
        string input,
        string? previousOutput,
        string output,
        out bool created)
    {
        EnsureActive();
        AgentPhasePolicy.EnsureBelongsTo(AgentType, phase);

        var phaseOutput = _phaseOutputs.FirstOrDefault(item => item.Phase == phase);
        if (phaseOutput is null)
        {
            phaseOutput = new ConversationPhaseOutput(Id, AgentType, phase, input, previousOutput, output);
            _phaseOutputs.Add(phaseOutput);
            created = true;
        }
        else
        {
            phaseOutput.Update(input, previousOutput, output);
            created = false;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return phaseOutput;
    }

    public ConversationMessage AddMessage(
        ConversationMessageRole role,
        ConversationMessageType type,
        AgentPhase? phase,
        string content)
    {
        EnsureActive();

        var message = new ConversationMessage(Id, AgentType, phase, role, type, content);
        _messages.Add(message);
        UpdatedAt = DateTimeOffset.UtcNow;
        return message;
    }

    public void Delete()
    {
        EnsureActive();
        IsDeleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureActive()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException(DomainMessages.ConversationDeleted);
        }
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
