using ChatApp.Application.Common.Exceptions;
using ChatApp.Application.Common.Interfaces;
using ChatApp.Domain.Interfaces;
using MediatR;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public class AddUserToRoomHandler : IRequestHandler<AddUserToRoomCommand, Unit>
{
    private readonly IChatRoomRepository _roomRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public AddUserToRoomHandler(
        IChatRoomRepository roomRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IUserRepository userRepository)
    {
        _roomRepository = roomRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(AddUserToRoomCommand request, CancellationToken ct)
    {
        var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var room = await _roomRepository.GetByIdAsync(request.RoomId)
            ?? throw new NotFoundException("ChatRoom", request.RoomId);

        if (!room.IsAdmin(currentUserId))
            throw new ForbiddenAccessException("Only the room admin can add users.");

        if (room.IsPrivate)
            throw new ForbiddenAccessException("Cannot add users to private chats.");

        var targetUser = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User", request.UserId);

        if (room.ParticipantIds.Contains(request.UserId))
            throw new ForbiddenAccessException("User is already a member of this room.");

        room.AddParticipant(request.UserId);
        _roomRepository.Update(room);
        await _unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
