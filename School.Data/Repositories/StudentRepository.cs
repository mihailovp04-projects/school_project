using Microsoft.EntityFrameworkCore;
using School.Domain;

namespace School.Data.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly SchoolDbContext _context;

    public StudentRepository(SchoolDbContext context)
    {
        _context = context;
    }

    public async Task<List<Student>> GetAllAsync()
    {
        return await _context.Students
            .Include(s => s.SchoolClass)
            .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.SchoolClass)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        var existing = await _context.Students.FindAsync(student.Id);
        if (existing == null)
        {
            return false;
        }

        existing.FirstName = student.FirstName;
        existing.LastName = student.LastName;
        existing.BirthDate = student.BirthDate;
        existing.Phone = student.Phone;
        existing.SchoolClassId = student.SchoolClassId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return false;
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }
}