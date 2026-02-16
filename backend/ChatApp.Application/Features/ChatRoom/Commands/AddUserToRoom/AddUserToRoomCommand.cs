using MediatR;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public record AddUserToRoomCommand(Guid RoomId, Guid UserId) : IRequest<Unit>;
