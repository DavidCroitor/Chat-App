using ChatApp.Domain.Common;
using ChatApp.Domain.Exceptions;
using ChatApp.Domain.ValueObjects;

namespace ChatApp.Domain.Entities;

public class User : BaseEntity, IAggregateRoot
{
    public string Username { get; private set; }
    public EmailAddress Email { get; private set; }
    public string PasswordHash { get; private set; }

    // Required for EF Core
    private User() { } 

    public User(string username, string email, string passwordHash)
    {
        Username = username;
        Email = new EmailAddress(email);
        PasswordHash = passwordHash;
    }

    public void UpdateUsername(string newUsername)
    {
        if (string.IsNullOrWhiteSpace(newUsername)) throw new DomainException("Username cannot be empty");
        Username = newUsername;
    }
}