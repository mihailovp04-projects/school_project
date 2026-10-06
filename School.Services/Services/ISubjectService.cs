using School.Domain;

namespace School.Services.Services;

public interface ISubjectService
{
    Task<List<Subject>> GetAllAsync();
    Task AddAsync(Subject subject);
    Task<bool> UpdateAsync(Subject subject);
    Task<bool> DeleteAsync(int id);
}