using Agents.Opi.Backend.Application.DTOs.Agent;
using Agents.Opi.Backend.Application.DTOs.Security;
using Agents.Opi.Backend.Domain.Enums;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Agent.Commands.StreamAgentPhase;

public sealed record StreamAgentPhaseCommand(
    AgentType AgentType,
    AgentPhase Phase,
    string Input,
    string? PreviousOutput,
    Guid? ConversationId,
    AuthenticatedUser User) : IRequest<AgentPhaseStreamResult>;
