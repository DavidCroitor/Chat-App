using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace ChatApp.Infrastructure.RealTime;

[Authorize]
public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _onlineUsers = new();

    private string? GetUserId() => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            var connections = _onlineUsers.GetOrAdd(userId, _ => new HashSet<string>());
            lock (connections)
            {
                var wasEmpty = connections.Count == 0;
                connections.Add(Context.ConnectionId);
                if (wasEmpty)
                {
                    // First connection for this user — broadcast online
                    Clients.Others.SendAsync("UserOnline", userId);
                }
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null && _onlineUsers.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(Context.ConnectionId);
                if (connections.Count == 0)
                {
                    _onlineUsers.TryRemove(userId, out _);
                    Clients.Others.SendAsync("UserOffline", userId);
                }
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    public Task<List<string>> GetOnlineUsers()
    {
        return Task.FromResult(_onlineUsers.Keys.ToList());
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task SendTyping(string roomId, string username)
    {
        await Clients.OthersInGroup(roomId)
            .SendAsync("UserTyping", roomId, username);
    }
}