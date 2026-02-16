using ChatApp.Domain.Exceptions;

namespace ChatApp.Domain.ValueObjects;

public record EmailAddress
{
    public string Value { get; init; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
            throw new DomainException("Invalid email format.");
            
        Value = value;
    }
}