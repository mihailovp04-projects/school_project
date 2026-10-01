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

    public async Task UpdateAsync(SchoolClass schoolClass)
    {
        var existing = await _context.SchoolClasses.FindAsync(schoolClass.Id);
        if (existing == null)
        {
            return;
        }

        existing.Name = schoolClass.Name;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var schoolClass = await _context.SchoolClasses.FindAsync(id);
        if (schoolClass != null)
        {
            _context.SchoolClasses.Remove(schoolClass);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddSubjectAsync(int classId, int subjectId)
    {
        var schoolClass = await _context.SchoolClasses
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == classId);

        var subject = await _context.Subjects.FindAsync(subjectId);

        if (schoolClass == null || subject == null)
        {
            return;
        }

        if (!schoolClass.Subjects.Any(s => s.Id == subjectId))
        {
            schoolClass.Subjects.Add(subject);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveSubjectAsync(int classId, int subjectId)
    {
        var schoolClass = await _context.SchoolClasses
            .Include(c => c.Subjects)
            .FirstOrDefaultAsync(c => c.Id == classId);

        if (schoolClass == null)
        {
            return;
        }

        var subject = schoolClass.Subjects.FirstOrDefault(s => s.Id == subjectId);
        if (subject != null)
        {
            schoolClass.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
        }
    }
}