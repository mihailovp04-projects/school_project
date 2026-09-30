using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class GradeService : IGradeService
{
    private readonly IGradeRepository _gradeRepository;
    private readonly ILogger<GradeService> _logger;

    public GradeService(IGradeRepository gradeRepository, ILogger<GradeService> logger)
    {
        _gradeRepository = gradeRepository;
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

    public async Task<double> GetAverageGradeAsync(int studentId)
    {
        var grades = await _gradeRepository.GetByStudentIdAsync(studentId);

        if (grades.Count == 0)
        {
            return 0;
        }

        return grades.Average(g => g.Value);
    }

    public async Task AddAsync(Grade grade)
    {
        ValidateGrade(grade);
        await _gradeRepository.AddAsync(grade);
        _logger.LogInformation("Grade added: StudentId {StudentId}, SubjectId {SubjectId}, Value {Value}", grade.StudentId, grade.SubjectId, grade.Value);
    }

    public async Task UpdateAsync(Grade grade)
    {
        ValidateGrade(grade);
        await _gradeRepository.UpdateAsync(grade);
        _logger.LogInformation("Grade updated: Id {Id}", grade.Id);
    }

    public async Task DeleteAsync(int id)
    {
        await _gradeRepository.DeleteAsync(id);
        _logger.LogInformation("Grade deleted: Id {Id}", id);
    }

    private void ValidateGrade(Grade grade)
    {
        if (grade.Value < 1 || grade.Value > 10)
        {
            _logger.LogWarning("Grade validation failed: value {Value} out of range", grade.Value);
            throw new ArgumentException("Оценка должна быть в диапазоне от 1 до 10.");
        }

        if (grade.Date > DateTime.Now)
        {
            _logger.LogWarning("Grade validation failed: date in the future");
            throw new ArgumentException("Дата оценки не может быть в будущем.");
        }
    }
}