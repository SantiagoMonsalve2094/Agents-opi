using Agents.Opi.Backend.Application.DTOs.Conversations;
using Agents.Opi.Backend.Application.DTOs.Security;
using Agents.Opi.Backend.Domain.Enums;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Conversations.Queries.GetConversationById;

public sealed record GetConversationByIdQuery(
    AgentType AgentType,
    Guid ConversationId,
    AuthenticatedUser User) : IRequest<ConversationDetailResponse>;
