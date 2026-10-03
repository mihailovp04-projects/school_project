using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;

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

    public async Task<Subject?> GetByIdAsync(int id)
    {
        return await _subjectRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(Subject subject)
    {
        ValidateSubject(subject);

        if (await _subjectRepository.ExistsByNameAsync(subject.Name))
        {
            throw new ArgumentException($"Предмет с названием \"{subject.Name}\" уже существует.");
        }

        await _subjectRepository.AddAsync(subject);
        _logger.LogInformation("Subject added: {Name} (Id: {Id})", subject.Name, subject.Id);
    }

    public async Task<bool> UpdateAsync(Subject subject)
    {
        ValidateSubject(subject);

        if (await _subjectRepository.ExistsByNameAsync(subject.Name, subject.Id))
        {
            throw new ArgumentException($"Предмет с названием \"{subject.Name}\" уже существует.");
        }

        var updated = await _subjectRepository.UpdateAsync(subject);
        if (updated)
        {
            _logger.LogInformation("Subject updated: Id {Id}", subject.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _subjectRepository.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Subject deleted: Id {Id}", id);
        }
        return deleted;
    }

    private void ValidateSubject(Subject subject)
    {
        if (string.IsNullOrWhiteSpace(subject.Name))
        {
            throw new ArgumentException("Название предмета обязательно для заполнения.");
        }
    }
}