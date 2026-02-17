using ChatApp.Domain.Entities;

namespace ChatApp.Domain.Interfaces;

public interface IChatRoomRepository
{
    Task<ChatRoom?> GetByIdAsync(Guid id);

    Task<ChatRoom?> GetWithRecentMessagesAsync(Guid id, int count);
    Task<ChatRoom?> GetPrivateChatByParticipantsAsync(Guid userA, Guid userB);
    Task<List<ChatRoom>> GetRoomsForUserAsync(Guid userId);

    Task AddAsync(ChatRoom chatRoom);

    void Update(ChatRoom chatRoom);

    void AddMessage(Message message);
    // For Chat History
    Task<List<Message>> GetPagedMessagesAsync(Guid roomId, int pageSize, DateTime? before);

    // For validation
    Task<bool> ExistsAsync(Guid roomId);

    // Unread counts
    Task<Dictionary<Guid, int>> GetUnreadCountsAsync(Guid userId);
    Task UpsertReadReceiptAsync(Guid userId, Guid roomId, DateTime readAt);
}