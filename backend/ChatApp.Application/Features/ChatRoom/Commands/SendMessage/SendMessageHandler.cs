using MediatR;
using FluentValidation;
using ChatApp.Domain.Interfaces;
using ChatApp.Application.Common.Interfaces;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.ValueObjects;
using ChatApp.Application.Common.Exceptions;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public class SendMessageHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IChatRoomRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IChatNotificationService _notificationService;

    public SendMessageHandler(
        IChatRoomRepository repository, 
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IChatNotificationService notificationService
    )
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var room = await _repository.GetByIdAsync(request.RoomId) 
                   ?? throw new NotFoundException("Chat room", request.RoomId);

        if (!room.ParticipantIds.Contains(userId))
            throw new UnauthorizedAccessException("You are not a participant of this room.");

        var message = room.AddMessage(new MessageContent(request.Text), userId);
        _repository.AddMessage(message);

        await _unitOfWork.SaveChangesAsync(ct);

        var sender = await _userRepository.GetByIdAsync(userId);
        var messageDto = new MessageDto(message.Id, message.Content.Value, message.SenderId, sender?.Username ?? "Unknown", message.CreatedAt);

        await _notificationService.NotifyNewMessage(request.RoomId, messageDto);

        return messageDto;
    }
}