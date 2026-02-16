using FluentValidation;
using MediatR;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.ValueObjects;

namespace ChatApp.Application.Features.Users.Commands;

public record RegisterUserCommand(string Username, string Email, string Password) : IRequest<AuthResponseDto>;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}
