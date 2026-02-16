using ChatApp.Domain.Common;
using ChatApp.Domain.Entities;

namespace ChatApp.Domain.Events;

public record MessageSentEvent(Message Message) : IDomainEvent;