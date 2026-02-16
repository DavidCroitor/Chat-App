using ChatApp.Application.Common.Interfaces;
using ChatApp.Application.Common.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Infrastructure.RealTime;

public class ChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatNotificationService(IHubContext<ChatHub> hubContext) 
        => _hubContext = hubContext;

    public async Task NotifyNewMessage(Guid roomId, MessageDto message)
    {
        await _hubContext.Clients.Group(roomId.ToString())
            .SendAsync("ReceiveMessage", message);
    }
}