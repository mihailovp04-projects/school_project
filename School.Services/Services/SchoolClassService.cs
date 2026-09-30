using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class SchoolClassService : ISchoolClassService
{
    private readonly ISchoolClassRepository _schoolClassRepository;
    private readonly ILogger<SchoolClassService> _logger;

    public SchoolClassService(ISchoolClassRepository schoolClassRepository, ILogger<SchoolClassService> logger)
    {
        _schoolClassRepository = schoolClassRepository;
        _logger = logger;
    }

    public async Task<List<SchoolClass>> GetAllAsync()
    {
        return await _schoolClassRepository.GetAllAsync();
    }

    public async Task<SchoolClass?> GetByIdAsync(int id)
    {
        return await _schoolClassRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(SchoolClass schoolClass)
    {
        ValidateSchoolClass(schoolClass);
        await CheckDuplicateNameAsync(schoolClass);
        await _schoolClassRepository.AddAsync(schoolClass);
        _logger.LogInformation("Class added: {Name} (Id: {Id})", schoolClass.Name, schoolClass.Id);
    }

    public async Task UpdateAsync(SchoolClass schoolClass)
    {
        ValidateSchoolClass(schoolClass);
        await _schoolClassRepository.UpdateAsync(schoolClass);
        _logger.LogInformation("Class updated: Id {Id}", schoolClass.Id);
    }

    public async Task DeleteAsync(int id)
    {
        await _schoolClassRepository.DeleteAsync(id);
        _logger.LogInformation("Class deleted: Id {Id}", id);
    }

    public async Task AssignSubjectAsync(int classId, int subjectId)
    {
        await _schoolClassRepository.AddSubjectAsync(classId, subjectId);
        _logger.LogInformation("Subject {SubjectId} assigned to class {ClassId}", subjectId, classId);
    }

    public async Task RemoveSubjectAsync(int classId, int subjectId)
    {
        await _schoolClassRepository.RemoveSubjectAsync(classId, subjectId);
        _logger.LogInformation("Subject {SubjectId} removed from class {ClassId}", subjectId, classId);
    }

    private void ValidateSchoolClass(SchoolClass schoolClass)
    {
        if (string.IsNullOrWhiteSpace(schoolClass.Name))
        {
            _logger.LogWarning("Class validation failed: empty name");
            throw new ArgumentException("Название класса обязательно для заполнения.");
        }
    }

    private async Task CheckDuplicateNameAsync(SchoolClass schoolClass)
    {
        var existingClasses = await _schoolClassRepository.GetAllAsync();

        var duplicate = existingClasses.Any(c =>
            string.Equals(c.Name, schoolClass.Name, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            _logger.LogWarning("Duplicate class name attempted: {Name}", schoolClass.Name);
            throw new ArgumentException($"Класс с названием \"{schoolClass.Name}\" уже существует.");
        }
    }
}