using School.Data.Contracts.Models;
using School.Domain;

namespace School.Services.Services;

public interface IAcademicTermService
{
    Task<List<AcademicTermSummary>> GetAllAsync();
    Task AddAsync(AcademicTerm term);
    Task<bool> UpdateAsync(AcademicTerm term);
    Task<bool> SetClosedAsync(int id, bool isClosed);
    Task<bool> DeleteAsync(int id);
}
