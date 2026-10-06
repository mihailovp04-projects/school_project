using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;
using School.Services.Validation;

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

    public async Task AddAsync(SchoolClass schoolClass)
    {
        EntityValidator.Validate(schoolClass);

        if (await _schoolClassRepository.ExistsByNameAsync(schoolClass.Name))
        {
            throw new ArgumentException($"Класс с названием \"{schoolClass.Name}\" уже существует.");
        }

        try
        {
            await _schoolClassRepository.AddAsync(schoolClass);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException($"Класс с названием \"{schoolClass.Name}\" уже существует.");
        }

        _logger.LogInformation("Class added: {Name} (Id: {Id})", schoolClass.Name, schoolClass.Id);
    }

    public async Task<bool> UpdateAsync(SchoolClass schoolClass)
    {
        EntityValidator.Validate(schoolClass);

        if (await _schoolClassRepository.ExistsByNameAsync(schoolClass.Name, schoolClass.Id))
        {
            throw new ArgumentException($"Класс с названием \"{schoolClass.Name}\" уже существует.");
        }

        bool updated;
        try
        {
            updated = await _schoolClassRepository.UpdateAsync(schoolClass);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException($"Класс с названием \"{schoolClass.Name}\" уже существует.");
        }

        if (updated)
        {
            _logger.LogInformation("Class updated: Id {Id}", schoolClass.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var deleted = await _schoolClassRepository.DeleteAsync(id);
            if (deleted)
            {
                _logger.LogInformation("Class deleted: Id {Id}", id);
            }
            return deleted;
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Нельзя удалить класс, в котором есть ученики.");
        }
    }

    public async Task<bool> AssignSubjectAsync(int classId, int subjectId)
    {
        var assigned = await _schoolClassRepository.AddSubjectAsync(classId, subjectId);
        if (assigned)
        {
            _logger.LogInformation("Subject {SubjectId} assigned to class {ClassId}", subjectId, classId);
        }
        return assigned;
    }

    public async Task<bool> RemoveSubjectAsync(int classId, int subjectId)
    {
        var removed = await _schoolClassRepository.RemoveSubjectAsync(classId, subjectId);
        if (removed)
        {
            _logger.LogInformation("Subject {SubjectId} removed from class {ClassId}", subjectId, classId);
        }
        return removed;
    }
}