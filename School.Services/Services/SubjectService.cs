using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;
using School.Domain.Exceptions;
using School.Services.Validation;

namespace School.Services.Services;

public class SubjectService : ISubjectService
{
    private const string SubjectInUseMessage = "Нельзя удалить предмет, по которому уже есть оценки.";

    private readonly ISubjectRepository _subjectRepository;
    private readonly ILogger<SubjectService> _logger;

    public SubjectService(ISubjectRepository subjectRepository, ILogger<SubjectService> logger)
    {
        _subjectRepository = subjectRepository;
        _logger = logger;
    }

    public async Task<List<Subject>> GetAllAsync()
    {
        return await _subjectRepository.GetAllAsync();
    }

    public async Task AddAsync(Subject subject)
    {
        EntityValidator.Validate(subject);

        if (await _subjectRepository.ExistsByNameAsync(subject.Name))
        {
            throw new ArgumentException(DuplicateNameMessage(subject.Name));
        }

        try
        {
            await _subjectRepository.AddAsync(subject);
        }
        catch (DuplicateNameException ex)
        {
            throw new ArgumentException(DuplicateNameMessage(subject.Name), ex);
        }

        _logger.LogInformation("Subject added: {Name} (Id: {Id})", subject.Name, subject.Id);
    }

    public async Task<bool> UpdateAsync(Subject subject)
    {
        EntityValidator.Validate(subject);

        if (await _subjectRepository.ExistsByNameAsync(subject.Name, subject.Id))
        {
            throw new ArgumentException(DuplicateNameMessage(subject.Name));
        }

        bool updated;
        try
        {
            updated = await _subjectRepository.UpdateAsync(subject);
        }
        catch (DuplicateNameException ex)
        {
            throw new ArgumentException(DuplicateNameMessage(subject.Name), ex);
        }

        if (updated)
        {
            _logger.LogInformation("Subject updated: Id {Id}", subject.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        bool deleted;
        try
        {
            deleted = await _subjectRepository.DeleteAsync(id);
        }
        catch (EntityInUseException ex)
        {
            throw new ArgumentException(SubjectInUseMessage, ex);
        }

        if (deleted)
        {
            _logger.LogInformation("Subject deleted: Id {Id}", id);
        }
        return deleted;
    }

    private static string DuplicateNameMessage(string name) => $"Предмет с названием \"{name}\" уже существует.";
}