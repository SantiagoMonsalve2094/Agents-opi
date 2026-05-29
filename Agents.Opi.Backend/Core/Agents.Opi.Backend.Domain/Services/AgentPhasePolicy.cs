using Agents.Opi.Backend.Domain.Enums;
using Agents.Opi.Backend.Domain.Resources;

namespace Agents.Opi.Backend.Domain.Services;

public static class AgentPhasePolicy
{
    public static bool BelongsTo(AgentType agentType, AgentPhase phase)
    {
        return agentType switch
        {
            AgentType.QA => phase is AgentPhase.Invest or AgentPhase.Istqb or AgentPhase.GherkinTable or AgentPhase.GherkinCode or AgentPhase.Selenium,
            AgentType.PO => phase is AgentPhase.Epic or AgentPhase.UserStories,
            _ => false
        };
    }

    public static void EnsureBelongsTo(AgentType agentType, AgentPhase phase)
    {
        if (!BelongsTo(agentType, phase))
        {
            throw new ArgumentException(DomainMessages.PhaseDoesNotBelongToAgent, nameof(phase));
        }
    }

    public static bool RequiresIstqbKnowledge(AgentType agentType, AgentPhase phase)
        => agentType == AgentType.QA && phase == AgentPhase.Istqb;
}
