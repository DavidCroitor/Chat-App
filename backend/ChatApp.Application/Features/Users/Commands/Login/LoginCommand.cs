using FluentValidation;
using MediatR;
using ChatApp.Application.Common.Interfaces;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.Interfaces;

namespace ChatApp.Application.Features.Users.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
