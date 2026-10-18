using School.Data.Contracts.Models;
using School.Domain;

namespace School.Services.Services;

public interface IStudentService
{
    Task<List<Student>> GetAllAsync();
    Task<PagedResult<Student>> GetPagedAsync(StudentFilter filter, int page, int pageSize);
    Task<int> CountAsync();
    Task<Student?> GetByIdAsync(int id);
    Task AddAsync(Student student);
    Task<bool> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
}