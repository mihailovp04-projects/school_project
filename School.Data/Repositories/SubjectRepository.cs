using Microsoft.EntityFrameworkCore;
using School.Domain;

namespace School.Data.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly SchoolDbContext _context;

    public SubjectRepository(SchoolDbContext context)
    {
        _context = context;
    }

    public async Task<List<Subject>> GetAllAsync()
    {
        return await _context.Subjects
            .Include(s => s.SchoolClasses)
            .ToListAsync();
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        return await _context.Subjects
            .Include(s => s.SchoolClasses)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Subject subject)
    {
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Subject subject)
    {
        var existing = await _context.Subjects.FindAsync(subject.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = subject.Name;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null)
        {
            return false;
        }

        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        return await _context.Subjects
            .Where(s => excludeId == null || s.Id != excludeId)
            .AnyAsync(s => s.Name.ToLower() == name.ToLower());
    }
}