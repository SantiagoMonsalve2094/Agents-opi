using Agents.Opi.Backend.Application.DTOs.Conversations;
using Agents.Opi.Backend.Application.DTOs.Security;
using Agents.Opi.Backend.Domain.Enums;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Conversations.Queries.GetConversations;

public sealed record GetConversationsQuery(
    AgentType AgentType,
    AuthenticatedUser User) : IRequest<IReadOnlyList<ConversationSummaryResponse>>;
