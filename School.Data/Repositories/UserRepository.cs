using Microsoft.EntityFrameworkCore;
using School.Data.Contracts.Repositories;
using School.Domain;

namespace School.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public UserRepository(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<User>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users
            .AsNoTracking()
            .OrderBy(u => u.Login)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task AddAsync(User user)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Users.Add(user);
        await context.SaveChangesCheckingUniqueAsync(user.Login);
    }

    public async Task<bool> UpdateRoleAsync(int id, string role)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Users.FindAsync(id);
        if (existing == null)
        {
            return false;
        }

        existing.Role = role;
        await context.SaveChangesAsync();
        return true;
    }
}