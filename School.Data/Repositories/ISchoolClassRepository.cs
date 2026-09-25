using School.Domain;

namespace School.Data.Repositories;

public interface ISchoolClassRepository
{
    Task<List<SchoolClass>> GetAllAsync();
    Task<SchoolClass?> GetByIdAsync(int id);
    Task AddAsync(SchoolClass schoolClass);
    Task UpdateAsync(SchoolClass schoolClass);
    Task DeleteAsync(int id);
}