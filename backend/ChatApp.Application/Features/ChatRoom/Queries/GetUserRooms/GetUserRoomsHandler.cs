using ChatApp.Application.Common.Interfaces;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.Interfaces;
using MediatR;

namespace ChatApp.Application.Features.ChatRooms.Queries;

public class GetUserRoomsHandler : IRequestHandler<GetUserRoomsQuery, List<ChatRoomDto>>
{
    private readonly IChatRoomRepository _roomRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepository;

    public GetUserRoomsHandler(
        IChatRoomRepository roomRepository,
        ICurrentUserService currentUser,
        IUserRepository userRepository
    )
    {
        _roomRepository = roomRepository;
        _currentUser = currentUser;
        _userRepository = userRepository;
    }

    public async Task<List<ChatRoomDto>> Handle(GetUserRoomsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var rooms = await _roomRepository.GetRoomsForUserAsync(userId);

        var roomDtos = new List<ChatRoomDto>();

        foreach (var room in rooms)
        {
            var participants = await _userRepository.GetByIdsAsync(room.ParticipantIds);
            var participantDtos = participants.Select(u => new ParticipantDto(u.Id, u.Username)).ToList();

            roomDtos.Add(new ChatRoomDto(
                room.Id,
                room.Name,
                room.IsPrivate,
                room.ParticipantIds.ToList(),
                participantDtos,
                room.CreatedAt,
                room.CreatorId
            ));
        }

        return roomDtos;
    }
}
