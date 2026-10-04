using Microsoft.EntityFrameworkCore;
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
        return await context.Users.AsNoTracking().ToListAsync();
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
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(User user)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Users.FindAsync(user.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Login = user.Login;
        existing.PasswordHash = user.PasswordHash;
        existing.Role = user.Role;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return true;
    }
}