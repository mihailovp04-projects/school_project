using School.Domain;

namespace School.Services.Services;

public interface IGradeService
{
    Task<List<Grade>> GetAllAsync();
    Task<Grade?> GetByIdAsync(int id);
    Task<List<Grade>> GetByStudentIdAsync(int studentId);
    Task<double> GetAverageGradeAsync(int studentId);
    Task AddAsync(Grade grade);
    Task UpdateAsync(Grade grade);
    Task DeleteAsync(int id);
}