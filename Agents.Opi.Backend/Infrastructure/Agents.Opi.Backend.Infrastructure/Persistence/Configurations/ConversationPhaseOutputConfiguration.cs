using Agents.Opi.Backend.Domain.Entities;
using Agents.Opi.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agents.Opi.Backend.Infrastructure.Persistence.Configurations;

public sealed class ConversationPhaseOutputConfiguration : IEntityTypeConfiguration<ConversationPhaseOutput>
{
    public void Configure(EntityTypeBuilder<ConversationPhaseOutput> builder)
    {
        builder.ToTable("conversation_phase_outputs");
        builder.HasKey(output => output.Id);
        builder.Property(output => output.Id).HasColumnName("id");
        builder.Property(output => output.ConversationId).HasColumnName("conversation_id").IsRequired();
        builder.Property(output => output.AgentType)
            .HasColumnName("agent_type")
            .HasConversion(value => value.ToString(), value => Enum.Parse<AgentType>(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(output => output.Phase)
            .HasColumnName("phase")
            .HasConversion(value => value.ToString(), value => Enum.Parse<AgentPhase>(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(output => output.Input).HasColumnName("input").IsRequired();
        builder.Property(output => output.PreviousOutput).HasColumnName("previous_output").IsRequired();
        builder.Property(output => output.Output).HasColumnName("output").IsRequired();
        builder.Property(output => output.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(output => output.CompletedAt).HasColumnName("completed_at").IsRequired();
        builder.HasIndex(output => new { output.ConversationId, output.Phase }).IsUnique();
    }
}
