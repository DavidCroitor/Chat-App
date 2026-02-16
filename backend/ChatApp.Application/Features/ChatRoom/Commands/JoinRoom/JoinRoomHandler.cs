using ChatApp.Application.Common.Exceptions;
using ChatApp.Application.Common.Interfaces;
using ChatApp.Domain.Interfaces;
using MediatR;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public class JoinRoomHandler : IRequestHandler<JoinRoomCommand, Unit>
{
    private readonly IChatRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public JoinRoomHandler(
        IChatRoomRepository roomRepository, 
        IUserRepository userRepository, 
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork    
    )
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(JoinRoomCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        
        var room = await _roomRepository.GetByIdAsync(request.RoomId) 
                   ?? throw new NotFoundException("Chat room not found.");

        var user = await _userRepository.GetByIdAsync(userId);
        
        // Use Domain Logic
        room.AddParticipant(user!.Id);

        await _unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}