using MediatR;
using FluentValidation;
using ChatApp.Application.Common.Models;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public record SendMessageCommand(Guid RoomId, string Text) : IRequest<MessageDto>;

public class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
    }
}