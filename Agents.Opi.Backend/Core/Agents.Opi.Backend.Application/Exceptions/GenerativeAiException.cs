namespace Agents.Opi.Backend.Application.Exceptions;

public sealed class GenerativeAiException(string message, string? providerDetails = null) : Exception(message)
{
    public string? ProviderDetails { get; } = providerDetails;
}
