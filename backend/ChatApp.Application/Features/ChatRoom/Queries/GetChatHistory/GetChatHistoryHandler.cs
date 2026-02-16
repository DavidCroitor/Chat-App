using MediatR;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.Interfaces;

namespace ChatApp.Application.Features.ChatRooms.Queries;

public class GetChatHistoryHandler : IRequestHandler<GetChatHistoryQuery, ChatHistoryDto>
{
    private readonly IChatRoomRepository _repository;
    private readonly IUserRepository _userRepository;

    public GetChatHistoryHandler(IChatRoomRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<ChatHistoryDto> Handle(GetChatHistoryQuery request, CancellationToken ct)
    {
        var room = await _repository.GetByIdAsync(request.RoomId);
        if (room is null)
            throw new Application.Common.Exceptions.NotFoundException("ChatRoom", request.RoomId);

        var messages = await _repository.GetPagedMessagesAsync(request.RoomId, request.PageSize, request.Before);

        var participantUsers = await _userRepository.GetByIdsAsync(room.ParticipantIds);
        var userLookup = participantUsers.ToDictionary(u => u.Id, u => u.Username);

        var messageDtos = messages.Select(m => new MessageDto(
            m.Id,
            m.Content.Value,
            m.SenderId,
            userLookup.GetValueOrDefault(m.SenderId, "Unknown"),
            m.CreatedAt)).ToList();

        var participants = participantUsers.Select(u => new ParticipantDto(
            u.Id,
            u.Username)).ToList();

        return new ChatHistoryDto(messageDtos, participants);
    }
}