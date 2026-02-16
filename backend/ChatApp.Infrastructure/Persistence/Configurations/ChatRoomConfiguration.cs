using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using ChatApp.Domain.Entities;

namespace ChatApp.Infrastructure.Persistence.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatorId)
            .IsRequired(false);

        var jsonOptions = new JsonSerializerOptions { WriteIndented = false };

        builder.Property(x => x.ParticipantIds)
            .HasField("_participantIds")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnType("uuid[]")
            .IsRequired();
        // Access the private List<Message> _messages
        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(x => x.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Map the navigation property to the private field
         builder.Metadata.FindNavigation(nameof(ChatRoom.Messages))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}