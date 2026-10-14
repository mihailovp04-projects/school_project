using School.Data.Contracts.Models;
using School.Domain;

namespace School.Data.Contracts.Repositories;

public interface IAcademicTermRepository
{
    Task<List<AcademicTermSummary>> GetAllWithGradeCountsAsync();
    Task<AcademicTerm?> GetByIdAsync(int id);
    Task<AcademicTerm?> GetByDateAsync(DateTime date);
    Task AddAsync(AcademicTerm term);
    Task<bool> UpdateAsync(AcademicTerm term);
    Task<bool> SetClosedAsync(int id, bool isClosed);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> HasOverlapAsync(DateTime startDate, DateTime endDate, int? excludeId = null);
    Task<bool> HasGradesAsync(DateTime startDate, DateTime endDate);
}
