using School.Domain;

namespace School.Services.Services;

public interface IStudentService
{
    Task<List<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(int id);
}