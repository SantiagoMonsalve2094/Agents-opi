using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.Interfaces;

public interface IAgentStrategy
{
    AgentType AgentType { get; }
    string BuildPrompt(AgentPhase phase, string input, string? previousOutput);
    string BuildFeedbackPrompt(AgentPhase phase, string input, string? previousOutput, string currentOutput, string feedback);
    bool RequiresKnowledge(AgentPhase phase);
}
