using Agents.Opi.Backend.Application.Features.Agent.Prompts;
using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Domain.Enums;
using Agents.Opi.Backend.Domain.Services;

namespace Agents.Opi.Backend.Application.Features.Agent.Strategies;

public sealed class PoAgentStrategy : IAgentStrategy
{
    public AgentType AgentType => AgentType.PO;

    public string BuildPrompt(AgentPhase phase, string input, string? previousOutput)
    {
        AgentPhasePolicy.EnsureBelongsTo(AgentType, phase);
        return OpiHuGeneratorAgentPromptBuilder.Build(phase, input, previousOutput);
    }

    public string BuildFeedbackPrompt(
        AgentPhase phase,
        string input,
        string? previousOutput,
        string currentOutput,
        string feedback)
    {
        AgentPhasePolicy.EnsureBelongsTo(AgentType, phase);
        return OpiHuGeneratorAgentPromptBuilder.BuildFeedback(phase, input, previousOutput, currentOutput, feedback);
    }

    public bool RequiresKnowledge(AgentPhase phase)
        => false;
}
