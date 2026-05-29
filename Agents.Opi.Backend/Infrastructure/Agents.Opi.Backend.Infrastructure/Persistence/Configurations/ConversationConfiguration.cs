using Agents.Opi.Backend.Domain.Entities;
using Agents.Opi.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agents.Opi.Backend.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");
        builder.HasKey(conversation => conversation.Id);
        builder.Property(conversation => conversation.Id).HasColumnName("id");
        builder.Property(conversation => conversation.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(conversation => conversation.AgentType)
            .HasColumnName("agent_type")
            .HasConversion(value => value.ToString(), value => Enum.Parse<AgentType>(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(conversation => conversation.Title).HasColumnName("title").HasMaxLength(120).IsRequired();
        builder.Property(conversation => conversation.InitialInput).HasColumnName("initial_input").IsRequired();
        builder.Property(conversation => conversation.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(conversation => conversation.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(conversation => conversation.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.HasMany(conversation => conversation.PhaseOutputs)
            .WithOne()
            .HasForeignKey(output => output.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(conversation => conversation.Messages)
            .WithOne()
            .HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<AgentUser>()
            .WithMany()
            .HasForeignKey(conversation => conversation.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(conversation => conversation.PhaseOutputs).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(conversation => conversation.Messages).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(conversation => new { conversation.UserId, conversation.AgentType, conversation.UpdatedAt });
    }
}
