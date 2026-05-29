using Agents.Opi.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agents.Opi.Backend.Infrastructure.Persistence;

public sealed class AgentDbContext(DbContextOptions<AgentDbContext> options) : DbContext(options)
{
    public DbSet<AgentUser> AgentUsers => Set<AgentUser>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<ConversationPhaseOutput> ConversationPhaseOutputs => Set<ConversationPhaseOutput>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgentDbContext).Assembly);
    }
}
