using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class StudentService : IStudentService
{
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

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _studentRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(Student student)
    {
        ValidateStudent(student);
        await _studentRepository.AddAsync(student);
        _logger.LogInformation("Student added: {FirstName} {LastName} (Id: {Id})", student.FirstName, student.LastName, student.Id);
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        ValidateStudent(student);
        var updated = await _studentRepository.UpdateAsync(student);
        if (updated)
        {
            _logger.LogInformation("Student updated: Id {Id}", student.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _studentRepository.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Student deleted: Id {Id}", id);
        }
        return deleted;
    }

    private void ValidateStudent(Student student)
    {
        if (string.IsNullOrWhiteSpace(student.FirstName) || string.IsNullOrWhiteSpace(student.LastName))
        {
            throw new ArgumentException("Имя и фамилия ученика обязательны для заполнения.");
        }

        if (student.BirthDate > DateTime.Now)
        {
            throw new ArgumentException("Дата рождения не может быть в будущем.");
        }
    }
}