namespace ChatApp.Application.Common.Models;

public record UserDto(
    Guid Id, 
    string Username, 
    string Email
);
