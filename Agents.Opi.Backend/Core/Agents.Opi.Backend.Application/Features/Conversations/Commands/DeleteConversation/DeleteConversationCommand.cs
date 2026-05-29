using Agents.Opi.Backend.Application.DTOs.Security;
using Agents.Opi.Backend.Domain.Enums;
using MediatR;

namespace Agents.Opi.Backend.Application.Features.Conversations.Commands.DeleteConversation;

public sealed record DeleteConversationCommand(
    AgentType AgentType,
    Guid ConversationId,
    AuthenticatedUser User) : IRequest;
