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
        await CheckDuplicateNameAsync(subject);
        await _subjectRepository.AddAsync(subject);
        _logger.LogInformation("Subject added: {Name} (Id: {Id})", subject.Name, subject.Id);
    }

    public async Task UpdateAsync(Subject subject)
    {
        ValidateSubject(subject);
        await _subjectRepository.UpdateAsync(subject);
        _logger.LogInformation("Subject updated: Id {Id}", subject.Id);
    }

    public async Task DeleteAsync(int id)
    {
        await _subjectRepository.DeleteAsync(id);
        _logger.LogInformation("Subject deleted: Id {Id}", id);
    }

    private void ValidateSubject(Subject subject)
    {
        if (string.IsNullOrWhiteSpace(subject.Name))
        {
            _logger.LogWarning("Subject validation failed: empty name");
            throw new ArgumentException("Название предмета обязательно для заполнения.");
        }
    }

    private async Task CheckDuplicateNameAsync(Subject subject)
    {
        var existingSubjects = await _subjectRepository.GetAllAsync();

        var duplicate = existingSubjects.Any(s =>
            string.Equals(s.Name, subject.Name, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            _logger.LogWarning("Duplicate subject name attempted: {Name}", subject.Name);
            throw new ArgumentException($"Предмет с названием \"{subject.Name}\" уже существует.");
        }
    }
}