using School.Data.Contracts.Models;
using School.Domain;

namespace School.Data.Contracts.Repositories;

public interface IGradeRepository
{
    Task<List<Grade>> GetByStudentIdAsync(int studentId);
    Task<Grade?> GetByIdAsync(int id);
    Task<List<GradeStat>> GetStatsForClassAsync(int schoolClassId, DateTime startDate, DateTime endDate);
    Task AddAsync(Grade grade);
    Task<bool> UpdateAsync(Grade grade);
    Task<bool> DeleteAsync(int id);
}