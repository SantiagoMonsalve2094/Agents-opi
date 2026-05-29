using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Agents.Opi.Backend.Infrastructure.Persistence;

public sealed class DesignTimeAgentDbContextFactory : IDesignTimeDbContextFactory<AgentDbContext>
{
    public AgentDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5435;Database=opi_agents_db;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AgentDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(AgentDbContext).Assembly.FullName))
            .Options;

        return new AgentDbContext(options);
    }
}
