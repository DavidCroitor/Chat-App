using ChatApp.Application.Common.Exceptions;
using ChatApp.Application.Common.Interfaces;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using MediatR;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public class CreatePrivateChatHandler : IRequestHandler<CreatePrivateChatCommand, ChatRoomDto>
{
    private readonly IChatRoomRepository _roomRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreatePrivateChatHandler(
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

    public async Task<ChatRoomDto> Handle(CreatePrivateChatCommand request, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        if (currentUserId == request.TargetUserId)
            throw new PrivateChatException("Cannot chat with yourself.");

        var existingRoom = await _roomRepository.GetPrivateChatByParticipantsAsync(
            currentUserId, 
            request.TargetUserId);

        if (existingRoom != null)
        {
            var existingParticipants = await _userRepository.GetByIdsAsync(existingRoom.ParticipantIds);
            var existingParticipantDtos = existingParticipants.Select(u => new ParticipantDto(u.Id, u.Username)).ToList();

            return new ChatRoomDto(
                existingRoom.Id,
                existingRoom.Name,
                existingRoom.IsPrivate,
                existingRoom.ParticipantIds.ToList(),
                existingParticipantDtos,
                existingRoom.CreatedAt,
                existingRoom.CreatorId);
        }

        var room = new ChatRoom(name: "Private Chat", isPrivate: true); 
        
        room.AddParticipant(currentUserId);
        room.AddParticipant(request.TargetUserId);

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