using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectService(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
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
        await _subjectRepository.AddAsync(subject);
    }

    public async Task UpdateAsync(Subject subject)
    {
        ValidateSubject(subject);
        await _subjectRepository.UpdateAsync(subject);
    }

    public async Task DeleteAsync(int id)
    {
        await _subjectRepository.DeleteAsync(id);
    }

    private void ValidateSubject(Subject subject)
    {
        if (string.IsNullOrWhiteSpace(subject.Name))
        {
            throw new ArgumentException("Название предмета обязательно для заполнения.");
        }
    }
}