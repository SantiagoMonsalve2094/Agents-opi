using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.Interfaces;

public interface IAgentStrategyResolver
{
    IAgentStrategy Resolve(AgentType agentType);
}
