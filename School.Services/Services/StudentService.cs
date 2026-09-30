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

    public async Task UpdateAsync(Student student)
    {
        ValidateStudent(student);
        await _studentRepository.UpdateAsync(student);
        _logger.LogInformation("Student updated: Id {Id}", student.Id);
    }

    public async Task DeleteAsync(int id)
    {
        await _studentRepository.DeleteAsync(id);
        _logger.LogInformation("Student deleted: Id {Id}", id);
    }

    private void ValidateStudent(Student student)
    {
        if (string.IsNullOrWhiteSpace(student.FirstName) || string.IsNullOrWhiteSpace(student.LastName))
        {
            _logger.LogWarning("Student validation failed: empty name or last name");
            throw new ArgumentException("Имя и фамилия ученика обязательны для заполнения.");
        }

        if (student.BirthDate > DateTime.Now)
        {
            _logger.LogWarning("Student validation failed: birth date in the future");
            throw new ArgumentException("Дата рождения не может быть в будущем.");
        }
    }
}