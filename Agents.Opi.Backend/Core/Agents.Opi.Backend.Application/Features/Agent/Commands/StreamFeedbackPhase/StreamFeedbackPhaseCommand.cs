using Agents.Opi.Backend.Application.DTOs.Agent;
using Agents.Opi.Backend.Application.DTOs.Security;
using Agents.Opi.Backend.Domain.Enums;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Agent.Commands.StreamFeedbackPhase;

public sealed record StreamFeedbackPhaseCommand(
    AgentType AgentType,
    AgentPhase Phase,
    Guid ConversationId,
    string Feedback,
    AuthenticatedUser User) : IRequest<AgentPhaseStreamResult>;
