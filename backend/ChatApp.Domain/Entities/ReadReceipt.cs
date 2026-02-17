namespace ChatApp.Domain.Entities;

public class ReadReceipt
{
    public Guid UserId { get; private set; }
    public Guid ChatRoomId { get; private set; }
    public DateTime LastReadAt { get; private set; }

    private ReadReceipt() { }

    public ReadReceipt(Guid userId, Guid chatRoomId, DateTime lastReadAt)
    {
        UserId = userId;
        ChatRoomId = chatRoomId;
        LastReadAt = lastReadAt;
    }

    public void UpdateLastReadAt(DateTime readAt)
    {
        LastReadAt = readAt;
    }
}
