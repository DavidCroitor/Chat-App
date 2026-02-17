using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

public class ChatRoomRepository : IChatRoomRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRoomRepository(ApplicationDbContext context) => _context = context;

    public async Task<ChatRoom?> GetByIdAsync(Guid id) 
        => await _context.ChatRooms
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(ChatRoom chatRoom) 
        => await _context.ChatRooms.AddAsync(chatRoom);

    public void Update(ChatRoom chatRoom)
    {
        _context.ChatRooms.Update(chatRoom);
    }

    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    // History Query
    public async Task<List<Message>> GetPagedMessagesAsync(Guid roomId, int pageSize, DateTime? before)
    {
        var query = _context.Messages.Where(m => m.ChatRoomId == roomId);
        
        if (before.HasValue) 
            query = query.Where(m => m.CreatedAt < before.Value);

        return await query.OrderByDescending(m => m.CreatedAt)
                          .Take(pageSize)
                          .ToListAsync();
    }

    public async Task<ChatRoom?> GetWithRecentMessagesAsync(Guid id, int count)
    {
        return await _context.ChatRooms
            .Include(x => x.Messages
                .OrderByDescending(m => m.CreatedAt) 
                .Take(count))                       
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ChatRooms.AnyAsync(x => x.Id == id);
    }
    public async Task<ChatRoom?> GetPrivateChatByParticipantsAsync(Guid userA, Guid userB)
    {
        return await _context.ChatRooms
            .FirstOrDefaultAsync(r => r.IsPrivate && 
                                    r.ParticipantIds.Contains(userA) && 
                                    r.ParticipantIds.Contains(userB));
    }

    public async Task<List<ChatRoom>> GetRoomsForUserAsync(Guid userId)
    {
        return await _context.ChatRooms
            .Where(r => r.ParticipantIds.Contains(userId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, int>> GetUnreadCountsAsync(Guid userId)
    {
        // Get all read receipts for this user
        var receipts = await _context.ReadReceipts
            .Where(r => r.UserId == userId)
            .ToDictionaryAsync(r => r.ChatRoomId, r => r.LastReadAt);

        // Get rooms for the user
        var roomIds = await _context.ChatRooms
            .Where(r => r.ParticipantIds.Contains(userId))
            .Select(r => r.Id)
            .ToListAsync();

        var counts = new Dictionary<Guid, int>();
        foreach (var roomId in roomIds)
        {
            var query = _context.Messages.Where(m => m.ChatRoomId == roomId && m.SenderId != userId);
            if (receipts.TryGetValue(roomId, out var lastRead))
            {
                query = query.Where(m => m.CreatedAt > lastRead);
            }
            counts[roomId] = await query.CountAsync();
        }

        return counts;
    }

    public async Task UpsertReadReceiptAsync(Guid userId, Guid roomId, DateTime readAt)
    {
        var receipt = await _context.ReadReceipts
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ChatRoomId == roomId);

        if (receipt != null)
        {
            receipt.UpdateLastReadAt(readAt);
        }
        else
        {
            _context.ReadReceipts.Add(new ReadReceipt(userId, roomId, readAt));
        }
    }

}