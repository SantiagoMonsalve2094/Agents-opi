using Agents.Opi.Backend.Domain.Resources;

namespace Agents.Opi.Backend.Domain.Entities;

public sealed class AgentUser
{
    private AgentUser()
    {
    }

    public AgentUser(string externalId, string email, string displayName)
    {
        Id = Guid.NewGuid();
        ExternalId = NormalizeRequired(externalId, nameof(externalId));
        Email = NormalizeEmail(email);
        DisplayName = displayName.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
        LastSeenAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }

    public void MarkSeen(string email, string displayName)
    {
        Email = NormalizeEmail(email);
        DisplayName = displayName.Trim();
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeEmail(string email)
        => NormalizeRequired(email, nameof(email)).ToLowerInvariant();

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(DomainMessages.RequiredValue, parameterName);
        }

        return value.Trim();
    }
}
