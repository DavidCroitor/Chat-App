namespace ChatApp.Application.Common.Models;

public record ChatRoomDto(
    Guid Id,
    string Name,
    bool IsPrivate,
    List<Guid> ParticipantIds,
    List<ParticipantDto> Participants,
    DateTime CreatedAt,
    Guid? CreatorId = null
);
