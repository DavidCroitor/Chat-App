using ChatApp.Application.Common.Interfaces;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.Events;
using ChatApp.Domain.Interfaces;
using MediatR;

public class MessageSentEventHandler : INotificationHandler<MessageSentEvent>
{
    private readonly IChatNotificationService _notificationService;
    private readonly IUserRepository _userRepository;

    public MessageSentEventHandler(IChatNotificationService notificationService, IUserRepository userRepository)
    {
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    public async Task Handle(MessageSentEvent notification, CancellationToken ct)
    {
        var sender = await _userRepository.GetByIdAsync(notification.Message.SenderId);

        var dto = new MessageDto(
            notification.Message.Id,
            notification.Message.Content.Value,
            notification.Message.SenderId,
            sender?.Username ?? "Unknown",
            notification.Message.CreatedAt);

        // Broadcast to SignalR
        await _notificationService.NotifyNewMessage(notification.Message.ChatRoomId, dto);
    }
}