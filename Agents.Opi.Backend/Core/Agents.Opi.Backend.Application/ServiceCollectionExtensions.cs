using Agents.Opi.Backend.Application.Features.Agent.Commands.GenerateAgentPhase;
using Agents.Opi.Backend.Application.Features.Agent.Commands.StreamAgentPhase;
using Agents.Opi.Backend.Application.Features.Agent.Strategies;
using Agents.Opi.Backend.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Agents.Opi.Backend.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddScoped<IRequestHandler<GenerateAgentPhaseCommand, DTOs.Agent.AgentPhaseResult>, GenerateAgentPhaseHandler>();
        services.AddScoped<IRequestHandler<StreamAgentPhaseCommand, DTOs.Agent.AgentPhaseStreamResult>, StreamAgentPhaseHandler>();
        services.AddScoped<IAgentStrategy, QaAgentStrategy>();
        services.AddScoped<IAgentStrategy, PoAgentStrategy>();
        services.AddScoped<IAgentStrategyResolver, AgentStrategyResolver>();

        return services;
    }
}
