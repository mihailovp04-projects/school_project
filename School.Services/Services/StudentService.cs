using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
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
    }

    public async Task UpdateAsync(Student student)
    {
        ValidateStudent(student);
        await _studentRepository.UpdateAsync(student);
    }

    public async Task DeleteAsync(int id)
    {
        await _studentRepository.DeleteAsync(id);
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