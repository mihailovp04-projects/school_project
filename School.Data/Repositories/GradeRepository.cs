using Microsoft.EntityFrameworkCore;
using School.Domain;

namespace School.Data.Repositories;

public class GradeRepository : IGradeRepository
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public GradeRepository(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Grade>> GetByStudentIdAsync(int studentId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Grades
            .Include(g => g.Subject)
            .Where(g => g.StudentId == studentId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Grade grade)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Grades.Add(grade);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Grade grade)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Grades.FindAsync(grade.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Value = grade.Value;
        existing.Date = grade.Date;
        existing.SubjectId = grade.SubjectId;
        existing.StudentId = grade.StudentId;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var grade = await context.Grades.FindAsync(id);
        if (grade == null)
        {
            return false;
        }

        context.Grades.Remove(grade);
        await context.SaveChangesAsync();
        return true;
    }
}