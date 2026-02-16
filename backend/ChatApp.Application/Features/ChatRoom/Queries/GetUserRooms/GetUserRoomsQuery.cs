using MediatR;
using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Features.ChatRooms.Queries;

public record GetUserRoomsQuery : IRequest<List<ChatRoomDto>>;
