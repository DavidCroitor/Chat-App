using MediatR;
using ChatApp.Application.Features.ChatRooms.Commands;
using ChatApp.Application.Features.ChatRooms.Queries;
using ChatApp.Application.Features.Users.Queries;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat").RequireAuthorization();

        // Messages
        group.MapPost("/rooms/{roomId:guid}/messages", async (Guid roomId, SendMessageRequest request, ISender mediator) =>
            Results.Ok(await mediator.Send(new SendMessageCommand(roomId, request.Text))));

        group.MapGet("/rooms/{roomId:guid}/history", async (Guid roomId, DateTime? before, int? pageSize, ISender mediator) =>
            Results.Ok(await mediator.Send(new GetChatHistoryQuery(roomId, pageSize ?? 50, before))));

        // Room Management
        group.MapPost("/rooms/private", async (CreatePrivateChatCommand command, ISender mediator) =>
            Results.Ok(await mediator.Send(command)));

        group.MapPost("/rooms/public", async (CreatePublicRoomCommand command, ISender mediator) =>
            Results.Ok(await mediator.Send(command)));

        group.MapPost("/rooms/join", async (JoinRoomCommand command, ISender mediator) =>
            Results.Ok(await mediator.Send(command)));

        // User Search
        group.MapGet("/users/search", async (string term, ISender mediator) =>
            Results.Ok(await mediator.Send(new SearchUsersQuery(term))));

        // User Rooms
        group.MapGet("/rooms", async (ISender mediator) =>
            Results.Ok(await mediator.Send(new GetUserRoomsQuery())));

        // Admin: Add user to room
        group.MapPost("/rooms/{roomId:guid}/users", async (Guid roomId, AddUserToRoomRequest request, ISender mediator) =>
            Results.Ok(await mediator.Send(new AddUserToRoomCommand(roomId, request.UserId))));

        // Mark room as read
        group.MapPost("/rooms/{roomId:guid}/read", async (Guid roomId, ISender mediator) =>
            Results.Ok(await mediator.Send(new MarkRoomAsReadCommand(roomId))));
    }
}

public record AddUserToRoomRequest(Guid UserId);

public record SendMessageRequest(string Text);