using School.Data.Contracts.Models;
using School.Data.Contracts.Repositories;

namespace School.Services.Reports;

public class TermReportService : ITermReportService
{
    private readonly IAcademicTermRepository _termRepository;
    private readonly ISchoolClassRepository _schoolClassRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IGradeRepository _gradeRepository;

    public TermReportService(
        IAcademicTermRepository termRepository,
        ISchoolClassRepository schoolClassRepository,
        IStudentRepository studentRepository,
        IGradeRepository gradeRepository)
    {
        _termRepository = termRepository;
        _schoolClassRepository = schoolClassRepository;
        _studentRepository = studentRepository;
        _gradeRepository = gradeRepository;
    }

    public async Task<TermReport?> BuildAsync(int termId, int schoolClassId)
    {
        var term = await _termRepository.GetByIdAsync(termId);
        if (term == null)
        {
            return null;
        }

        var schoolClass = await _schoolClassRepository.GetByIdAsync(schoolClassId);
        if (schoolClass == null)
        {
            return null;
        }

        var students = await _studentRepository.GetByClassIdAsync(schoolClassId);
        var stats = await _gradeRepository.GetStatsForClassAsync(schoolClassId, term.StartDate, term.EndDate);
        var subjects = schoolClass.Subjects.OrderBy(s => s.Name).ToList();
        var statsByStudent = stats.ToLookup(s => s.StudentId);

        var rows = students
            .Select(student =>
            {
                var studentStats = statsByStudent[student.Id].ToList();
                var average = Average(studentStats);
                return new TermReportRow
                {
                    StudentName = $"{student.LastName} {student.FirstName}",
                    SubjectAverages = subjects.ToDictionary(s => s.Id, s => Average(studentStats.Where(x => x.SubjectId == s.Id))),
                    Average = average,
                    FinalGrade = average.HasValue ? GradeMath.ToFinalGrade(average.Value) : null
                };
            })
            .ToList();

        return new TermReport
        {
            Term = term,
            SchoolClass = schoolClass,
            Subjects = subjects,
            Rows = rows,
            SubjectAverages = subjects.ToDictionary(s => s.Id, s => Average(stats.Where(x => x.SubjectId == s.Id))),
            HasGrades = stats.Count > 0
        };
    }

    private static double? Average(IEnumerable<GradeStat> stats)
    {
        var sum = 0;
        var count = 0;
        foreach (var stat in stats)
        {
            sum += stat.Sum;
            count += stat.Count;
        }

        return count == 0 ? null : (double)sum / count;
    }
}
