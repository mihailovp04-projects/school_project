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
}