namespace ChatApp.Application.Common.Models;
public record MessageDto(
    Guid Id, 
    string Content, 
    Guid SenderId,
    string SenderUsername,
    DateTime SentAt
);