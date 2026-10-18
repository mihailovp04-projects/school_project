using School.Data.Contracts.Models;
using School.Domain;

namespace School.Data.Contracts.Repositories;

public interface IStudentRepository
{
    Task<List<Student>> GetAllAsync();
    Task<PagedResult<Student>> GetPagedAsync(StudentFilter filter, int page, int pageSize);
    Task<int> CountAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<List<Student>> GetByClassIdAsync(int schoolClassId);
    Task AddAsync(Student student);
    Task<bool> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
}