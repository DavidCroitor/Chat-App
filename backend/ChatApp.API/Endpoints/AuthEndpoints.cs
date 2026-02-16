using MediatR;
using ChatApp.Application.Features.Users.Commands;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterUserCommand command, ISender mediator) => 
            Results.Ok(await mediator.Send(command)));

        group.MapPost("/login", async (LoginCommand command, ISender mediator) => 
            Results.Ok(await mediator.Send(command)));
    }
}