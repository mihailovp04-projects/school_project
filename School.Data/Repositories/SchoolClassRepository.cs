using Microsoft.EntityFrameworkCore;
using School.Domain;

namespace School.Data.Repositories;

public class SchoolClassRepository : ISchoolClassRepository
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public SchoolClassRepository(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<SchoolClass>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SchoolClasses
            .Include(c => c.Students)
            .Include(c => c.Subjects)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SchoolClass?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SchoolClasses
            .Include(c => c.Students)
            .Include(c => c.Subjects)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(SchoolClass schoolClass)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.SchoolClasses.Add(schoolClass);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(SchoolClass schoolClass)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.SchoolClasses.FindAsync(schoolClass.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = schoolClass.Name;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var schoolClass = await context.SchoolClasses.FindAsync(id);
        if (schoolClass == null)
        {
            return false;
        }

        context.SchoolClasses.Remove(schoolClass);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SchoolClasses
            .Where(c => excludeId == null || c.Id != excludeId)
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> AddSubjectAsync(int classId, int subjectId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var schoolClass = await context.SchoolClasses
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == classId);
        var subject = await context.Subjects.FindAsync(subjectId);

        if (schoolClass == null || subject == null)
        {
            return false;
        }

        if (!schoolClass.Subjects.Any(s => s.Id == subjectId))
        {
            schoolClass.Subjects.Add(subject);
            await context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> RemoveSubjectAsync(int classId, int subjectId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var schoolClass = await context.SchoolClasses
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == classId);

        if (schoolClass == null)
        {
            return false;
        }

        var subject = schoolClass.Subjects.FirstOrDefault(s => s.Id == subjectId);
        if (subject == null)
        {
            return false;
        }

        schoolClass.Subjects.Remove(subject);
        await context.SaveChangesAsync();
        return true;
    }
}