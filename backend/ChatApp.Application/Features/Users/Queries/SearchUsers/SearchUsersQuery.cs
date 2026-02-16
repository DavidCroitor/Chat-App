using ChatApp.Application.Common.Models;
using MediatR;

namespace ChatApp.Application.Features.Users.Queries;

public record SearchUsersQuery(string SearchTerm) : IRequest<List<SearchUserDto>>;