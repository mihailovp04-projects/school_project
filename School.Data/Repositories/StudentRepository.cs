using Microsoft.EntityFrameworkCore;
using School.Domain;

namespace School.Data.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public StudentRepository(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Student>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Students
            .Include(s => s.SchoolClass)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Students
            .Include(s => s.SchoolClass)
                .ThenInclude(c => c.Subjects)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Student student)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Students.Add(student);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Students.FindAsync(student.Id);
        if (existing == null)
        {
            return false;
        }

        existing.FirstName = student.FirstName;
        existing.LastName = student.LastName;
        existing.BirthDate = student.BirthDate;
        existing.Phone = student.Phone;
        existing.SchoolClassId = student.SchoolClassId;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var student = await context.Students.FindAsync(id);
        if (student == null)
        {
            return false;
        }

        context.Students.Remove(student);
        await context.SaveChangesAsync();
        return true;
    }
}