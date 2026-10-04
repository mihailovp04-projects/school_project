using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class GradeService : IGradeService
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<GradeService> _logger;

    public GradeService(IGradeRepository gradeRepository, IStudentRepository studentRepository, ILogger<GradeService> logger)
    {
        _gradeRepository = gradeRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<List<Grade>> GetAllAsync()
    {
        return await _gradeRepository.GetAllAsync();
    }

    public async Task<Grade?> GetByIdAsync(int id)
    {
        return await _gradeRepository.GetByIdAsync(id);
    }

    public async Task<List<Grade>> GetByStudentIdAsync(int studentId)
    {
        return await _gradeRepository.GetByStudentIdAsync(studentId);
    }

    public async Task<double?> GetAverageGradeAsync(int studentId)
    {
        var grades = await _gradeRepository.GetByStudentIdAsync(studentId);

        if (grades.Count == 0)
        {
            return null;
        }

        return grades.Average(g => g.Value);
    }

    public async Task AddAsync(Grade grade)
    {
        await ValidateGradeAsync(grade);
        await _gradeRepository.AddAsync(grade);
        _logger.LogInformation("Grade added: StudentId {StudentId}, SubjectId {SubjectId}, Value {Value}", grade.StudentId, grade.SubjectId, grade.Value);
    }

    public async Task<bool> UpdateAsync(Grade grade)
    {
        await ValidateGradeAsync(grade);
        var updated = await _gradeRepository.UpdateAsync(grade);
        if (updated)
        {
            _logger.LogInformation("Grade updated: Id {Id}", grade.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _gradeRepository.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Grade deleted: Id {Id}", id);
        }
        return deleted;
    }

    private async Task ValidateGradeAsync(Grade grade)
    {
        if (grade.Value < 1 || grade.Value > 10)
        {
            throw new ArgumentException("Оценка должна быть в диапазоне от 1 до 10.");
        }

        if (grade.Date == default || grade.Date > DateTime.Now)
        {
            throw new ArgumentException("Дата оценки не может быть в будущем.");
        }

        var student = await _studentRepository.GetByIdAsync(grade.StudentId);
        if (student == null)
        {
            throw new ArgumentException("Ученик не найден.");
        }

        if (!student.SchoolClass.Subjects.Any(s => s.Id == grade.SubjectId))
        {
            throw new ArgumentException("Этот предмет не привязан к классу ученика.");
        }
    }
}