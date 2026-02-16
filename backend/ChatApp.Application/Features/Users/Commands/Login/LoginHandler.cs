using MediatR;
using ChatApp.Application.Common.Interfaces;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.Interfaces;

namespace ChatApp.Application.Features.Users.Commands;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        bool isValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username);

        return new AuthResponseDto(user.Id, user.Username, user.Email.Value, token);
    }
}