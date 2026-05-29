using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Application.Resources;
using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.Features.Agent.Strategies;

public sealed class AgentStrategyResolver(IEnumerable<IAgentStrategy> strategies) : IAgentStrategyResolver
{
    public IAgentStrategy Resolve(AgentType agentType)
    {
        return strategies.FirstOrDefault(strategy => strategy.AgentType == agentType)
            ?? throw new ArgumentException(ApplicationMessages.InvalidAgentType, nameof(agentType));
    }
}
