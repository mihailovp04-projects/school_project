using Microsoft.EntityFrameworkCore;
using School.Domain;

namespace School.Data.Repositories;

public class SchoolClassRepository : ISchoolClassRepository
{
    private readonly SchoolDbContext _context;

    public SchoolClassRepository(SchoolDbContext context)
    {
        _context = context;
    }

    public async Task<List<SchoolClass>> GetAllAsync()
    {
        return await _context.SchoolClasses
            .Include(c => c.Students)
            .Include(c => c.Subjects)
            .ToListAsync();
    }

    public async Task<SchoolClass?> GetByIdAsync(int id)
    {
        return await _context.SchoolClasses
            .Include(c => c.Students)
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(SchoolClass schoolClass)
    {
        _context.SchoolClasses.Add(schoolClass);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(SchoolClass schoolClass)
    {
        var existing = await _context.SchoolClasses.FindAsync(schoolClass.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = schoolClass.Name;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var schoolClass = await _context.SchoolClasses.FindAsync(id);
        if (schoolClass == null)
        {
            return false;
        }

        _context.SchoolClasses.Remove(schoolClass);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        return await _context.SchoolClasses
            .Where(c => excludeId == null || c.Id != excludeId)
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> AddSubjectAsync(int classId, int subjectId)
    {
        var schoolClass = await _context.SchoolClasses
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == classId);
        var subject = await _context.Subjects.FindAsync(subjectId);

        if (schoolClass == null || subject == null)
        {
            return false;
        }

        if (!schoolClass.Subjects.Any(s => s.Id == subjectId))
        {
            schoolClass.Subjects.Add(subject);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> RemoveSubjectAsync(int classId, int subjectId)
    {
        var schoolClass = await _context.SchoolClasses
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
        await _context.SaveChangesAsync();
        return true;
    }
}