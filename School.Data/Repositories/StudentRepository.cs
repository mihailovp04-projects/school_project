using Microsoft.EntityFrameworkCore;
using School.Data.Contracts.Models;
using School.Data.Contracts.Repositories;
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
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PagedResult<Student>> GetPagedAsync(StudentFilter filter, int page, int pageSize)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        IQueryable<Student> query = context.Students.AsNoTracking();

        if (filter.SchoolClassId.HasValue)
        {
            var schoolClassId = filter.SchoolClassId.Value;
            query = query.Where(s => s.SchoolClassId == schoolClassId);
        }

        if (!string.IsNullOrWhiteSpace(filter.LastName))
        {
            var lastName = filter.LastName.Trim();
            query = query.Where(s => s.LastName.Contains(lastName));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(s => s.SchoolClass)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ThenBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Student>(items, totalCount, page, pageSize);
    }

    public async Task<int> CountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Students.CountAsync();
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

    public async Task<List<Student>> GetByClassIdAsync(int schoolClassId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Students
            .Where(s => s.SchoolClassId == schoolClassId)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .AsNoTracking()
            .ToListAsync();
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
        await context.SaveChangesCheckingReferencesAsync();
        return true;
    }
}