using MediatR;
using ChatApp.Application.Common.Models;
using FluentValidation;

namespace ChatApp.Application.Features.ChatRooms.Commands;

public record CreatePublicRoomCommand(string Name) : IRequest<ChatRoomDto>;

public class CreatePublicRoomValidator : AbstractValidator<CreatePublicRoomCommand>
{
    public CreatePublicRoomValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
    }
}
