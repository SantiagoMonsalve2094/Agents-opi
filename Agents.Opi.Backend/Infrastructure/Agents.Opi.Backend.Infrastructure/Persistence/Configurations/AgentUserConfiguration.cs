using Agents.Opi.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agents.Opi.Backend.Infrastructure.Persistence.Configurations;

public sealed class AgentUserConfiguration : IEntityTypeConfiguration<AgentUser>
{
    public void Configure(EntityTypeBuilder<AgentUser> builder)
    {
        builder.ToTable("agent_users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasColumnName("id");
        builder.Property(user => user.ExternalId).HasColumnName("external_id").HasMaxLength(256).IsRequired();
        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
        builder.Property(user => user.DisplayName).HasColumnName("display_name").HasMaxLength(256).IsRequired();
        builder.Property(user => user.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(user => user.LastSeenAt).HasColumnName("last_seen_at").IsRequired();
        builder.HasIndex(user => user.ExternalId).IsUnique();
        builder.HasIndex(user => user.Email);
    }
}
