using ChatApp.Application.Common.Interfaces;
using ChatApp.Domain.Interfaces;
using MediatR;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public class MarkRoomAsReadHandler : IRequestHandler<MarkRoomAsReadCommand, Unit>
{
    private readonly IChatRoomRepository _roomRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public MarkRoomAsReadHandler(
        IChatRoomRepository roomRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(MarkRoomAsReadCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        await _roomRepository.UpsertReadReceiptAsync(userId, request.RoomId, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
