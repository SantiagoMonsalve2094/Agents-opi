using Microsoft.IdentityModel.Tokens;

namespace Agents.Opi.Backend.Api.Security;

public static class ExtensionSecretOptions
{
    private static readonly char[] Separators = [';', ','];

    public static IReadOnlyCollection<SecurityKey> GetSigningKeys(IConfiguration configuration)
    {
        var secrets = GetConfiguredSecrets(configuration)
            .Select(secret => secret.Trim())
            .Where(secret => !string.IsNullOrWhiteSpace(secret))
            .Distinct(StringComparer.Ordinal)
            .Select(ExtensionSecretKeyFactory.Create)
            .Cast<SecurityKey>()
            .ToArray();

        return secrets;
    }

    private static IEnumerable<string> GetConfiguredSecrets(IConfiguration configuration)
    {
        foreach (var secret in configuration.GetSection("AzureDevOps:ExtensionSecrets").Get<string[]>() ?? [])
        {
            yield return secret;
        }

        foreach (var secret in Split(configuration["AzureDevOps:ExtensionSecret"]))
        {
            yield return secret;
        }

        foreach (var secret in Split(Environment.GetEnvironmentVariable("AZURE_DEVOPS_EXTENSION_SECRETS")))
        {
            yield return secret;
        }

        foreach (var secret in Split(Environment.GetEnvironmentVariable("AZURE_DEVOPS_EXTENSION_SECRET")))
        {
            yield return secret;
        }
    }

    private static IEnumerable<string> Split(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (var item in value.Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return item;
        }
    }
}
