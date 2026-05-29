using Agents.Opi.Backend.Application.DTOs.Agent;

namespace Agents.Opi.Backend.Application.Interfaces;

public interface IAgentGenerationPort
{
    Task<string> GenerateAsync(GenerativeAiRequest request, CancellationToken cancellationToken);
    IAsyncEnumerable<string> GenerateStreamAsync(GenerativeAiRequest request, CancellationToken cancellationToken);
}
