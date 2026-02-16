using MediatR;
using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Features.ChatRooms.Queries;

public record GetChatHistoryQuery(Guid RoomId, int PageSize = 50, DateTime? Before = null) 
    : IRequest<ChatHistoryDto>;