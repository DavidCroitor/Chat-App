using MediatR;
using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public record CreatePrivateChatCommand(Guid TargetUserId) : IRequest<ChatRoomDto>;