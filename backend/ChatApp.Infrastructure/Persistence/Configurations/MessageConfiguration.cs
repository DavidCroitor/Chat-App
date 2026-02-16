using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatApp.Domain.Entities;
using ChatApp.Domain.ValueObjects;

namespace ChatApp.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(x => x.Id);

        // FIX: Use ONLY HasConversion. Remove OwnsOne entirely.
        builder.Property(m => m.Content)
            .HasConversion(
                content => content.Value,             // How to save to DB
                value => new MessageContent(value))   // How to read from DB
            .HasColumnName("Content")                 // SQL Column Name
            .HasMaxLength(1000)                       // SQL Constraint
            .IsRequired();

        builder.Property(x => x.SenderId).IsRequired();
        builder.Property(x => x.ChatRoomId).IsRequired();
    }
}