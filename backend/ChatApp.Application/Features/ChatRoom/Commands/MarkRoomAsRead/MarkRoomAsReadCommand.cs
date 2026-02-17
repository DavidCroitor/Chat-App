using MediatR;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public record MarkRoomAsReadCommand(Guid RoomId) : IRequest<Unit>;
