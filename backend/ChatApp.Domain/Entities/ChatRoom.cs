using ChatApp.Domain.Common;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Events;
using ChatApp.Domain.Exceptions;
using ChatApp.Domain.ValueObjects;

namespace ChatApp.Domain.Entities;

public class ChatRoom : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public bool IsPrivate { get; private set; }
    public Guid? CreatorId { get; private set; }
    
    private readonly List<Message> _messages = new();
    private readonly List<Guid> _participantIds = new();
    public IReadOnlyCollection<Guid> ParticipantIds => _participantIds.AsReadOnly();
    
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    private ChatRoom(){}

    public ChatRoom(string name, bool isPrivate, Guid? creatorId = null)
    {
        Name = name;
        IsPrivate = isPrivate;
        CreatorId = creatorId;
    }

    public bool IsAdmin(Guid userId) => CreatorId.HasValue && CreatorId.Value == userId;

    public Message AddMessage(MessageContent content, Guid senderId)
    {
        var message = new Message(content, senderId, Id);
        
        _messages.Add(message);
        
        AddDomainEvent(new MessageSentEvent(message));

        return message;
    }

   public void AddParticipant(Guid userId)
    {
        if (IsPrivate && _participantIds.Count >= 2)
            throw new DomainException("Private chats cannot have more than 2 participants.");

        if (!_participantIds.Contains(userId))
            _participantIds.Add(userId);
    }
}