using Microsoft.EntityFrameworkCore;
using School.Data.Contracts.Models;
using School.Data.Contracts.Repositories;
using School.Domain;

namespace School.Data.Repositories;

public class AcademicTermRepository : IAcademicTermRepository
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public AcademicTermRepository(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<AcademicTermSummary>> GetAllWithGradeCountsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var rows = await context.AcademicTerms
            .AsNoTracking()
            .OrderBy(t => t.StartDate)
            .Select(t => new
            {
                Term = t,
                GradeCount = context.Grades.Count(g => g.Date >= t.StartDate && g.Date <= t.EndDate)
            })
            .ToListAsync();

        return rows.Select(r => new AcademicTermSummary(r.Term, r.GradeCount)).ToList();
    }

    public async Task<AcademicTerm?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AcademicTerms
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<AcademicTerm?> GetByDateAsync(DateTime date)
    {
        var day = date.Date;
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AcademicTerms
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.StartDate <= day && t.EndDate >= day);
    }

    public async Task AddAsync(AcademicTerm term)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.AcademicTerms.Add(term);
        await context.SaveChangesCheckingUniqueAsync(term.Name);
    }

    public async Task<bool> UpdateAsync(AcademicTerm term)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.AcademicTerms.FindAsync(term.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = term.Name;
        existing.StartDate = term.StartDate;
        existing.EndDate = term.EndDate;

        await context.SaveChangesCheckingUniqueAsync(term.Name);
        return true;
    }

    public async Task<bool> SetClosedAsync(int id, bool isClosed)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.AcademicTerms.FindAsync(id);
        if (existing == null)
        {
            return false;
        }

        existing.IsClosed = isClosed;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.AcademicTerms.FindAsync(id);
        if (existing == null)
        {
            return false;
        }

        context.AcademicTerms.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        IQueryable<AcademicTerm> query = context.AcademicTerms;
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(t => t.Name == name);
    }

    public async Task<bool> HasOverlapAsync(DateTime startDate, DateTime endDate, int? excludeId = null)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        await using var context = await _contextFactory.CreateDbContextAsync();
        IQueryable<AcademicTerm> query = context.AcademicTerms;
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(t => t.StartDate <= end && t.EndDate >= start);
    }

    public async Task<bool> HasGradesAsync(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Grades.AnyAsync(g => g.Date >= start && g.Date <= end);
    }
}
