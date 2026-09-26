using School.Domain;

namespace School.Services.Services;

public interface ISchoolClassService
{
    Task<List<SchoolClass>> GetAllAsync();
    Task<SchoolClass?> GetByIdAsync(int id);
    Task AddAsync(SchoolClass schoolClass);
    Task UpdateAsync(SchoolClass schoolClass);
    Task DeleteAsync(int id);
}