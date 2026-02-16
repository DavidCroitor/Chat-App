using ChatApp.Application.Common.Models;
using ChatApp.Application.Features.Users.Queries;
using ChatApp.Domain.Interfaces;
using MediatR;

public class SearchUsersHandler : IRequestHandler<SearchUsersQuery, List<SearchUserDto>>
{
    private readonly IUserRepository _userRepository;

    public SearchUsersHandler(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<List<SearchUserDto>> Handle(SearchUsersQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm)) return new();

        var users = await _userRepository.SearchByUsernameAsync(request.SearchTerm);
        return users.Select(u => new SearchUserDto(u.Id, u.Username)).ToList();
    }
}