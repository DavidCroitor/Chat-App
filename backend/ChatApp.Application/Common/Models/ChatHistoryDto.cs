namespace ChatApp.Application.Common.Models;

public record ChatHistoryDto(
    List<MessageDto> Messages,
    List<ParticipantDto> Participants
);
