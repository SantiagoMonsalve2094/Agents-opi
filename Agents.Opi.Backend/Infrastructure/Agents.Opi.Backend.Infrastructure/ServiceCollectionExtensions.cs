using Agents.Opi.Backend.Application.Interfaces;
using Agents.Opi.Backend.Infrastructure.GenerativeAi.OpenAi;
using Agents.Opi.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agents.Opi.Backend.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection("OpenAI"));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5435;Database=opi_agents_db;Username=postgres;Password=postgres";

        services.AddDbContext<AgentDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(AgentDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient<IAgentGenerationPort, OpenAiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/v1/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        return services;
    }
}
