using MediatR;
using ChatApp.Application.Common.Interfaces;
using ChatApp.Application.Common.Models;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using ChatApp.Application.Common.Exceptions;

namespace ChatApp.Application.Features.Users.Commands;


public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserHandler(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher, 
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork    
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new UserAlreadyExistsException("Email is already in use.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Username, request.Email, passwordHash);

        await _userRepository.AddAsync(user);

        await _unitOfWork.SaveChangesAsync(ct);

        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Username);
        
        return new AuthResponseDto(user.Id, user.Username, user.Email.Value, token);
    }
}