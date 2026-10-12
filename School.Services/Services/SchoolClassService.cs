using Microsoft.Extensions.Logging;
using School.Data.Contracts.Exceptions;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Validation;

namespace School.Services.Services;

public class SchoolClassService : ISchoolClassService
{
    private const string ClassInUseMessage = "Нельзя удалить класс, в котором есть ученики.";

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
            throw new ArgumentException(DuplicateNameMessage(schoolClass.Name));
        }

        try
        {
            await _schoolClassRepository.AddAsync(schoolClass);
        }
        catch (DuplicateNameException ex)
        {
            throw new ArgumentException(DuplicateNameMessage(schoolClass.Name), ex);
        }

        _logger.LogInformation("Class added: {Name} (Id: {Id})", schoolClass.Name, schoolClass.Id);
    }

    public async Task<bool> UpdateAsync(SchoolClass schoolClass)
    {
        EntityValidator.Validate(schoolClass);

        if (await _schoolClassRepository.ExistsByNameAsync(schoolClass.Name, schoolClass.Id))
        {
            throw new ArgumentException(DuplicateNameMessage(schoolClass.Name));
        }

        bool updated;
        try
        {
            updated = await _schoolClassRepository.UpdateAsync(schoolClass);
        }
        catch (DuplicateNameException ex)
        {
            throw new ArgumentException(DuplicateNameMessage(schoolClass.Name), ex);
        }

        if (updated)
        {
            _logger.LogInformation("Class updated: Id {Id}", schoolClass.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        bool deleted;
        try
        {
            deleted = await _schoolClassRepository.DeleteAsync(id);
        }
        catch (EntityInUseException ex)
        {
            throw new ArgumentException(ClassInUseMessage, ex);
        }

        if (deleted)
        {
            _logger.LogInformation("Class deleted: Id {Id}", id);
        }
        return deleted;
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

    private static string DuplicateNameMessage(string name) => $"Класс с названием \"{name}\" уже существует.";
}
