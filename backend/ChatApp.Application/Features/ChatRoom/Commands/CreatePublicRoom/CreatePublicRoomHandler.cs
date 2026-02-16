using ChatApp.Application.Common.Interfaces;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using MediatR;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public class CreatePublicRoomHandler : IRequestHandler<CreatePublicRoomCommand, ChatRoomDto>
{
    private readonly IChatRoomRepository _roomRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreatePublicRoomHandler(
        IChatRoomRepository roomRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IUserRepository userRepository
    )
    {
        _roomRepository = roomRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<ChatRoomDto> Handle(CreatePublicRoomCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var room = new ChatRoom(request.Name, isPrivate: false, creatorId: userId);
        room.AddParticipant(userId);

        await _roomRepository.AddAsync(room);
        await _unitOfWork.SaveChangesAsync(ct);

        var participants = await _userRepository.GetByIdsAsync(room.ParticipantIds);
        var participantDtos = participants.Select(u => new ParticipantDto(u.Id, u.Username)).ToList();

        return new ChatRoomDto(
            room.Id,
            room.Name,
            room.IsPrivate,
            room.ParticipantIds.ToList(),
            participantDtos,
            room.CreatedAt,
            room.CreatorId);
    }
}
