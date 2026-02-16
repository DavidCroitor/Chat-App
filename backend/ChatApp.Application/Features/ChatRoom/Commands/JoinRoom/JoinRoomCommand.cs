using MediatR;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public record JoinRoomCommand(Guid RoomId) : IRequest<Unit>;