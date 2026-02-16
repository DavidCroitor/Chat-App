using ChatApp.Domain.Exceptions;

namespace ChatApp.Domain.ValueObjects;

public record MessageContent
{
    public string Value { get; init; }

    public MessageContent(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Message cannot be empty.");
            
        if (value.Length > 1000)
            throw new DomainException("Message is too long.");

        Value = value;
    }
}