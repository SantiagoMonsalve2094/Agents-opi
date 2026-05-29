namespace Agents.Opi.Backend.Application.DTOs.Security;

public sealed record AuthenticatedUser(
    string ExternalId,
    string Email,
    string DisplayName);
