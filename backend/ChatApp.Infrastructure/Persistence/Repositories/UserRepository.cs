using Microsoft.EntityFrameworkCore;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Domain.ValueObjects;

namespace ChatApp.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        // Since Email is a Value Object, EF will compare the underlying string
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == new EmailAddress(email));
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    // Search logic for the SearchUsersQuery
    public async Task<IEnumerable<User>> SearchByUsernameAsync(string term)
    {
        return await _context.Users
            .Where(u => EF.Functions.ILike(u.Username, $"%{term}%")) // ILike is Postgres-specific (Case-Insensitive)
            .Take(20) // Limit results for performance
            .AsNoTracking() // Queries are faster when we don't need to save changes to the objects
            .ToListAsync();
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}