using Agents.Opi.Backend.Api.Resources;
using Agents.Opi.Backend.Domain.Enums;
using Agents.Opi.Backend.Domain.Services;

namespace Agents.Opi.Backend.Api.Mapping;

public static class AgentRequestParser
{
    public static AgentType ParseAgentType(string value)
    {
        if (Enum.TryParse<AgentType>(value, ignoreCase: true, out var agentType))
        {
            return agentType;
        }

        throw new ArgumentException(ApiMessages.InvalidAgentType, nameof(value));
    }

    public static AgentPhase ParsePhase(AgentType agentType, string value)
    {
        var phase = value.Trim().ToLowerInvariant() switch
        {
            "invest" => AgentPhase.Invest,
            "istqb" => AgentPhase.Istqb,
            "gherkin_table" => AgentPhase.GherkinTable,
            "gherkin_code" => AgentPhase.GherkinCode,
            "selenium" => AgentPhase.Selenium,
            "epic" => AgentPhase.Epic,
            "user_stories" => AgentPhase.UserStories,
            _ => throw new ArgumentException(ApiMessages.InvalidPhase, nameof(value))
        };

        AgentPhasePolicy.EnsureBelongsTo(agentType, phase);
        return phase;
    }
}
