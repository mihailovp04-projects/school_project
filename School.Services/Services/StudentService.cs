using Microsoft.Extensions.Logging;
using School.Data.Contracts.Exceptions;
using School.Data.Contracts.Models;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Validation;

namespace School.Services.Services;

public class StudentService : IStudentService
{
    private const string StudentInUseMessage = "Нельзя удалить ученика, у которого есть оценки.";
    private const int MaxPageSize = 100;

    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository studentRepository, ILogger<StudentService> logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<List<Student>> GetAllAsync()
    {
        return await _studentRepository.GetAllAsync();
    }

    public async Task<PagedResult<Student>> GetPagedAsync(StudentFilter filter, int page, int pageSize)
    {
        var size = Math.Clamp(pageSize, 1, MaxPageSize);
        var current = Math.Max(page, 1);

        var result = await _studentRepository.GetPagedAsync(filter, current, size);
        if (result.Items.Count == 0 && result.TotalCount > 0 && current > result.TotalPages)
        {
            result = await _studentRepository.GetPagedAsync(filter, result.TotalPages, size);
        }

        return result;
    }

    public async Task<int> CountAsync()
    {
        return await _studentRepository.CountAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _studentRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(Student student)
    {
        EntityValidator.Validate(student);
        await _studentRepository.AddAsync(student);
        _logger.LogInformation("Student added: {FirstName} {LastName} (Id: {Id})", student.FirstName, student.LastName, student.Id);
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        EntityValidator.Validate(student);
        var updated = await _studentRepository.UpdateAsync(student);
        if (updated)
        {
            _logger.LogInformation("Student updated: Id {Id}", student.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        bool deleted;
        try
        {
            deleted = await _studentRepository.DeleteAsync(id);
        }
        catch (EntityInUseException ex)
        {
            throw new ArgumentException(StudentInUseMessage, ex);
        }

        if (deleted)
        {
            _logger.LogInformation("Student deleted: Id {Id}", id);
        }
        return deleted;
    }
}
