using ChatApp.Domain.Common;
using ChatApp.Domain.Events;
using ChatApp.Domain.Exceptions;
using ChatApp.Domain.ValueObjects;

namespace ChatApp.Domain.Entities;

public class Message : BaseEntity
{
    public MessageContent Content { get; private set; } 
    public Guid SenderId { get; private set; }
    public Guid ChatRoomId { get; private set; }
    
    public User Sender { get; private set; } = null!;
    private Message(){}

    public Message(MessageContent content, Guid senderId, Guid chatRoomId)
    {   
        Content = content;
        SenderId = senderId;
        ChatRoomId = chatRoomId;
    }
}