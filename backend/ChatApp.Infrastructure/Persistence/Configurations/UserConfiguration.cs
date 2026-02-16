using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatApp.Domain.Entities;
using ChatApp.Domain.ValueObjects;

namespace ChatApp.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        // FIX: Use ONLY HasConversion. Remove OwnsOne entirely.
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,             // To string for DB
                value => new EmailAddress(value)) // Back to record for C#
            .HasColumnName("Email")
            .IsRequired();

        // Now you can safely create the unique index on the Email property
        builder.HasIndex(u => u.Email).IsUnique();
        
        builder.HasIndex(u => u.Username);

        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(u => u.PasswordHash)
            .IsRequired();
    }
}