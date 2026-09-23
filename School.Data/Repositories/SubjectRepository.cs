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

    public async Task UpdateAsync(Subject subject)
    {
        _context.Subjects.Update(subject);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject != null)
        {
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
        }
    }
}