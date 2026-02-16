using ChatApp.Domain.Entities;

namespace ChatApp.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task<IEnumerable<User>> SearchByUsernameAsync(string term);
}