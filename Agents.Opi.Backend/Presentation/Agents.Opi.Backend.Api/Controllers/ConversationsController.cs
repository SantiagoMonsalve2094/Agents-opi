using Agents.Opi.Backend.Api.Mapping;
using Agents.Opi.Backend.Api.Security;
using Agents.Opi.Backend.Application.DTOs.Conversations;
using Agents.Opi.Backend.Application.Features.Conversations.Commands.DeleteConversation;
using Agents.Opi.Backend.Application.Features.Conversations.Queries.GetConversationById;
using Agents.Opi.Backend.Application.Features.Conversations.Queries.GetConversations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agents.Opi.Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/conversations")]
public sealed class ConversationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationSummaryResponse>>> GetConversations(
        [FromQuery] string agentType,
        CancellationToken cancellationToken)
    {
        var parsedAgentType = AgentRequestParser.ParseAgentType(agentType);
        var user = AuthenticatedUserResolver.FromHttpContext(HttpContext);
        var result = await mediator.Send(new GetConversationsQuery(parsedAgentType, user), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationDetailResponse>> GetConversationById(
        Guid id,
        [FromQuery] string agentType,
        CancellationToken cancellationToken)
    {
        var parsedAgentType = AgentRequestParser.ParseAgentType(agentType);
        var user = AuthenticatedUserResolver.FromHttpContext(HttpContext);
        var result = await mediator.Send(new GetConversationByIdQuery(parsedAgentType, id, user), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteConversation(
        Guid id,
        [FromQuery] string agentType,
        CancellationToken cancellationToken)
    {
        var parsedAgentType = AgentRequestParser.ParseAgentType(agentType);
        var user = AuthenticatedUserResolver.FromHttpContext(HttpContext);
        await mediator.Send(new DeleteConversationCommand(parsedAgentType, id, user), cancellationToken);
        return NoContent();
    }
}
