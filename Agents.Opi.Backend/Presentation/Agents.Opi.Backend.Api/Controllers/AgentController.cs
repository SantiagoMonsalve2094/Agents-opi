using Agents.Opi.Backend.Api.DTOs;
using Agents.Opi.Backend.Api.Mapping;
using Agents.Opi.Backend.Api.Resources;
using Agents.Opi.Backend.Api.Security;
using Agents.Opi.Backend.Application.Features.Agent.Commands.GenerateAgentPhase;
using Agents.Opi.Backend.Application.Features.Agent.Commands.StreamFeedbackPhase;
using Agents.Opi.Backend.Application.Features.Agent.Commands.StreamAgentPhase;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agents.Opi.Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/agent")]
public sealed class AgentController(IMediator mediator) : ControllerBase
{
    [HttpPost("phase")]
    public async Task<ActionResult<AgentPhaseApiResponse>> GeneratePhase(
        AgentPhaseApiRequest request,
        CancellationToken cancellationToken)
    {
        var agentType = AgentRequestParser.ParseAgentType(request.AgentType);
        var phase = AgentRequestParser.ParsePhase(agentType, request.Phase);
        var user = AuthenticatedUserResolver.FromHttpContext(HttpContext);

        var result = await mediator.Send(
            new GenerateAgentPhaseCommand(
                agentType,
                phase,
                request.Input,
                request.PreviousOutput,
                request.ConversationId,
                user),
            cancellationToken);

        return Ok(new AgentPhaseApiResponse(result.Output, result.ConversationId));
    }

    [HttpPost("phase/stream")]
    public async Task StreamPhase(AgentPhaseApiRequest request, CancellationToken cancellationToken)
    {
        var agentType = AgentRequestParser.ParseAgentType(request.AgentType);
        var phase = AgentRequestParser.ParsePhase(agentType, request.Phase);
        var user = AuthenticatedUserResolver.FromHttpContext(HttpContext);

        var result = await mediator.Send(
            new StreamAgentPhaseCommand(
                agentType,
                phase,
                request.Input,
                request.PreviousOutput,
                request.ConversationId,
                user),
            cancellationToken);

        Response.Headers[ApiHeaders.ConversationId] = result.ConversationId.ToString();
        Response.ContentType = "text/plain; charset=utf-8";

        await foreach (var chunk in result.Stream.WithCancellation(cancellationToken))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    [HttpPost("phase/feedback/stream")]
    public async Task StreamFeedback(AgentPhaseFeedbackApiRequest request, CancellationToken cancellationToken)
    {
        var agentType = AgentRequestParser.ParseAgentType(request.AgentType);
        var phase = AgentRequestParser.ParsePhase(agentType, request.Phase);
        var user = AuthenticatedUserResolver.FromHttpContext(HttpContext);

        var result = await mediator.Send(
            new StreamFeedbackPhaseCommand(
                agentType,
                phase,
                request.ConversationId,
                request.Feedback,
                user),
            cancellationToken);

        Response.Headers[ApiHeaders.ConversationId] = result.ConversationId.ToString();
        Response.ContentType = "text/plain; charset=utf-8";

        await foreach (var chunk in result.Stream.WithCancellation(cancellationToken))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
