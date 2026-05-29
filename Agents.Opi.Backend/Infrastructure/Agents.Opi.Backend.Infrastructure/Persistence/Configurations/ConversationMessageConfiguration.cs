using Agents.Opi.Backend.Domain.Entities;
using Agents.Opi.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agents.Opi.Backend.Infrastructure.Persistence.Configurations;

public sealed class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("conversation_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).HasColumnName("id");
        builder.Property(message => message.ConversationId).HasColumnName("conversation_id").IsRequired();
        builder.Property(message => message.AgentType)
            .HasColumnName("agent_type")
            .HasConversion(value => value.ToString(), value => Enum.Parse<AgentType>(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(message => message.Phase)
            .HasColumnName("phase")
            .HasConversion(
                value => value.HasValue ? value.Value.ToString() : null,
                value => string.IsNullOrWhiteSpace(value) ? null : Enum.Parse<AgentPhase>(value))
            .HasMaxLength(32);
        builder.Property(message => message.Role)
            .HasColumnName("role")
            .HasConversion(value => value.ToString(), value => Enum.Parse<ConversationMessageRole>(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(message => message.Type)
            .HasColumnName("type")
            .HasConversion(value => value.ToString(), value => Enum.Parse<ConversationMessageType>(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(message => message.Content).HasColumnName("content").IsRequired();
        builder.Property(message => message.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(message => new { message.ConversationId, message.CreatedAt });
    }
}
