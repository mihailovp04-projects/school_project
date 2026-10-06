using School.Domain;

namespace School.Services.Services;

public interface ISchoolClassService
{
    Task<List<SchoolClass>> GetAllAsync();
    Task AddAsync(SchoolClass schoolClass);
    Task<bool> UpdateAsync(SchoolClass schoolClass);
    Task<bool> DeleteAsync(int id);
    Task<bool> AssignSubjectAsync(int classId, int subjectId);
    Task<bool> RemoveSubjectAsync(int classId, int subjectId);
}