using Microsoft.EntityFrameworkCore;
using School.Domain;

namespace School.Data.Repositories;

public class GradeRepository : IGradeRepository
{
    private readonly SchoolDbContext _context;

    public GradeRepository(SchoolDbContext context)
    {
        _context = context;
    }

    public async Task<List<Grade>> GetAllAsync()
    {
        return await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .ToListAsync();
    }

    public async Task<Grade?> GetByIdAsync(int id)
    {
        return await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<Grade>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Grades
            .Include(g => g.Subject)
            .Where(g => g.StudentId == studentId)
            .ToListAsync();
    }

    public async Task AddAsync(Grade grade)
    {
        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Grade grade)
    {
        _context.Grades.Update(grade);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade != null)
        {
            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
        }
    }
}