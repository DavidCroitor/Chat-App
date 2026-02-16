using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Common.Interfaces;

public interface IChatNotificationService {
    Task NotifyNewMessage(Guid roomId, MessageDto message);
}