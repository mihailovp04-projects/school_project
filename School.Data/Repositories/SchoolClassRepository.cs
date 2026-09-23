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
        _context.SchoolClasses.Update(schoolClass);
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
}