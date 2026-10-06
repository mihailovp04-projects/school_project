using School.Domain;

namespace School.Data.Repositories;

public interface ISchoolClassRepository
{
    Task<List<SchoolClass>> GetAllAsync();
    Task AddAsync(SchoolClass schoolClass);
    Task<bool> UpdateAsync(SchoolClass schoolClass);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> AddSubjectAsync(int classId, int subjectId);
    Task<bool> RemoveSubjectAsync(int classId, int subjectId);
}