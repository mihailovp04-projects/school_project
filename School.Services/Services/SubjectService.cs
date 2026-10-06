using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;
using School.Services.Validation;

namespace School.Services.Services;

public class SubjectService : ISubjectService
{
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
            throw new ArgumentException($"Предмет с названием \"{subject.Name}\" уже существует.");
        }

        try
        {
            await _subjectRepository.AddAsync(subject);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException($"Предмет с названием \"{subject.Name}\" уже существует.");
        }

        _logger.LogInformation("Subject added: {Name} (Id: {Id})", subject.Name, subject.Id);
    }

    public async Task<bool> UpdateAsync(Subject subject)
    {
        EntityValidator.Validate(subject);

        if (await _subjectRepository.ExistsByNameAsync(subject.Name, subject.Id))
        {
            throw new ArgumentException($"Предмет с названием \"{subject.Name}\" уже существует.");
        }

        bool updated;
        try
        {
            updated = await _subjectRepository.UpdateAsync(subject);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException($"Предмет с названием \"{subject.Name}\" уже существует.");
        }

        if (updated)
        {
            _logger.LogInformation("Subject updated: Id {Id}", subject.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var deleted = await _subjectRepository.DeleteAsync(id);
            if (deleted)
            {
                _logger.LogInformation("Subject deleted: Id {Id}", id);
            }
            return deleted;
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Нельзя удалить предмет, по которому уже есть оценки.");
        }
    }
}