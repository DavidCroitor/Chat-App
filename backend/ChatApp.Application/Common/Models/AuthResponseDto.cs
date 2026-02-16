namespace ChatApp.Application.Common.Models;

public record AuthResponseDto(
    Guid Id,
    string Username,
    string Email,
    string Token
);