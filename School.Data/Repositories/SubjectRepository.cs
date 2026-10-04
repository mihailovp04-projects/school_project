using Microsoft.EntityFrameworkCore;
using School.Domain;

namespace School.Data.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public SubjectRepository(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Subject>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Subjects
            .Include(s => s.SchoolClasses)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Subjects
            .Include(s => s.SchoolClasses)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Subject subject)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Subject subject)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Subjects.FindAsync(subject.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = subject.Name;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var subject = await context.Subjects.FindAsync(id);
        if (subject == null)
        {
            return false;
        }

        context.Subjects.Remove(subject);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Subjects
            .Where(s => excludeId == null || s.Id != excludeId)
            .AnyAsync(s => s.Name.ToLower() == name.ToLower());
    }
}