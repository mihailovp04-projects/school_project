using Microsoft.Extensions.Logging;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Validation;

namespace School.Services.Services;

public class GradeService : IGradeService
{
    private const string OutsideTermsMessage = "Дата оценки не попадает ни в один учебный период. Сначала создайте период на странице «Учебные периоды».";

    private readonly IGradeRepository _gradeRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAcademicTermRepository _termRepository;
    private readonly ILogger<GradeService> _logger;

    public GradeService(
        IGradeRepository gradeRepository,
        IStudentRepository studentRepository,
        IAcademicTermRepository termRepository,
        ILogger<GradeService> logger)
    {
        _gradeRepository = gradeRepository;
        _studentRepository = studentRepository;
        _termRepository = termRepository;
        _logger = logger;
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
        grade.Date = grade.Date.Date;
        await ValidateGradeAsync(grade);
        await _gradeRepository.AddAsync(grade);
        _logger.LogInformation("Grade added: StudentId {StudentId}, SubjectId {SubjectId}, Value {Value}", grade.StudentId, grade.SubjectId, grade.Value);
    }

    public async Task<bool> UpdateAsync(Grade grade)
    {
        grade.Date = grade.Date.Date;
        await ValidateGradeAsync(grade);

        var existing = await _gradeRepository.GetByIdAsync(grade.Id);
        if (existing == null)
        {
            return false;
        }

        await EnsureNotInClosedTermAsync(existing.Date);

        var updated = await _gradeRepository.UpdateAsync(grade);
        if (updated)
        {
            _logger.LogInformation("Grade updated: Id {Id}", grade.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _gradeRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return false;
        }

        await EnsureNotInClosedTermAsync(existing.Date);

        var deleted = await _gradeRepository.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Grade deleted: Id {Id}", id);
        }
        return deleted;
    }

    private async Task ValidateGradeAsync(Grade grade)
    {
        EntityValidator.Validate(grade);

        var student = await _studentRepository.GetByIdAsync(grade.StudentId);
        if (student == null)
        {
            throw new ArgumentException("Ученик не найден.");
        }

        if (!student.SchoolClass.Subjects.Any(s => s.Id == grade.SubjectId))
        {
            throw new ArgumentException("Этот предмет не привязан к классу ученика.");
        }

        var term = await _termRepository.GetByDateAsync(grade.Date);
        if (term == null)
        {
            throw new ArgumentException(OutsideTermsMessage);
        }

        if (term.IsClosed)
        {
            throw new ArgumentException(ClosedTermMessage(term.Name));
        }
    }

    private async Task EnsureNotInClosedTermAsync(DateTime date)
    {
        var term = await _termRepository.GetByDateAsync(date);
        if (term is { IsClosed: true })
        {
            throw new ArgumentException(ClosedTermMessage(term.Name));
        }
    }

    private static string ClosedTermMessage(string termName) => $"Период «{termName}» закрыт: оценки в нём менять нельзя.";
}
