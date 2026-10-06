using School.Domain;

namespace School.Data.Repositories;

public interface ISubjectRepository
{
    Task<List<Subject>> GetAllAsync();
    Task AddAsync(Subject subject);
    Task<bool> UpdateAsync(Subject subject);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
}