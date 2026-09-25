using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class GradeService : IGradeService
{
    private readonly IGradeRepository _gradeRepository;

    public GradeService(IGradeRepository gradeRepository)
    {
        _gradeRepository = gradeRepository;
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
    }

    public async Task UpdateAsync(Grade grade)
    {
        ValidateGrade(grade);
        await _gradeRepository.UpdateAsync(grade);
    }

    public async Task DeleteAsync(int id)
    {
        await _gradeRepository.DeleteAsync(id);
    }

    private void ValidateGrade(Grade grade)
    {
        if (grade.Value < 1 || grade.Value > 10)
        {
            throw new ArgumentException("Оценка должна быть в диапазоне от 1 до 10.");
        }
    }
}