using System.Security.Claims;
using Agents.Opi.Backend.Api.Resources;
using Agents.Opi.Backend.Application.DTOs.Security;

namespace Agents.Opi.Backend.Api.Security;

public static class AuthenticatedUserResolver
{
    private static readonly string[] ExternalIdClaimTypes = ["sub", ClaimTypes.NameIdentifier, "nameid", "oid", "appid", "aud"];
    private static readonly string[] EmailClaimTypes = ["preferred_username", "upn", "email", ClaimTypes.Email, "name"];

    public static AuthenticatedUser FromHttpContext(HttpContext context)
    {
        var externalId = FirstClaim(context.User, ExternalIdClaimTypes);
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new UnauthorizedAccessException(ApiMessages.Unauthorized);
        }

        var email = FirstClaim(context.User, EmailClaimTypes)
            ?? ReadHeader(context, ApiHeaders.AzureUserEmail)
            ?? BuildTechnicalEmail(externalId);

        var displayName = FirstClaim(context.User, "name", ClaimTypes.Name)
            ?? ReadHeader(context, ApiHeaders.AzureUserName)
            ?? string.Empty;

        return new AuthenticatedUser(externalId, email, displayName);
    }

    private static string? FirstClaim(ClaimsPrincipal principal, params string[] claimTypes)
        => claimTypes.Select(type => principal.FindFirst(type)?.Value).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static string? ReadHeader(HttpContext context, string name)
    {
        if (!context.Request.Headers.TryGetValue(name, out var value))
        {
            return null;
        }

        var rawValue = value.ToString().Trim();
        return string.IsNullOrWhiteSpace(rawValue) ? null : Uri.UnescapeDataString(rawValue);
    }

    private static string BuildTechnicalEmail(string externalId)
    {
        var normalized = string.Concat(externalId.Select(character =>
            char.IsLetterOrDigit(character) || character is '.' or '_' or '-' ? character : '_'));

        return $"{(string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized)}@azuredevops.local";
    }
}
