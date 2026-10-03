using School.Domain;

namespace School.Services.Services;

public interface ISchoolClassService
{
    Task<List<SchoolClass>> GetAllAsync();
    Task<SchoolClass?> GetByIdAsync(int id);
    Task AddAsync(SchoolClass schoolClass);
    Task<bool> UpdateAsync(SchoolClass schoolClass);
    Task<bool> DeleteAsync(int id);
    Task AssignSubjectAsync(int classId, int subjectId);
    Task RemoveSubjectAsync(int classId, int subjectId);
}