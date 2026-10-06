using School.Domain;

namespace School.Data.Repositories;

public interface IGradeRepository
{
    Task<List<Grade>> GetByStudentIdAsync(int studentId);
    Task AddAsync(Grade grade);
    Task<bool> UpdateAsync(Grade grade);
    Task<bool> DeleteAsync(int id);
}