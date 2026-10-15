using Moq;
using School.Data.Contracts.Models;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Reports;

namespace School.Tests;

public class TermReportServiceTests
{
    private const int TermId = 1;
    private const int ClassId = 5;
    private const int MathId = 10;
    private const int HistoryId = 20;

    private static readonly AcademicTerm Term = new()
    {
        Id = TermId,
        Name = "I четверть",
        StartDate = new DateTime(2026, 9, 1),
        EndDate = new DateTime(2026, 10, 31)
    };

    private readonly Mock<IAcademicTermRepository> _termRepositoryMock = new();
    private readonly Mock<ISchoolClassRepository> _schoolClassRepositoryMock = new();
    private readonly Mock<IStudentRepository> _studentRepositoryMock = new();
    private readonly Mock<IGradeRepository> _gradeRepositoryMock = new();
    private readonly TermReportService _service;

    public TermReportServiceTests()
    {
        _service = new TermReportService(
            _termRepositoryMock.Object,
            _schoolClassRepositoryMock.Object,
            _studentRepositoryMock.Object,
            _gradeRepositoryMock.Object);

        _termRepositoryMock.Setup(r => r.GetByIdAsync(TermId)).ReturnsAsync(Term);
        _schoolClassRepositoryMock.Setup(r => r.GetByIdAsync(ClassId)).ReturnsAsync(new SchoolClass
        {
            Id = ClassId,
            Name = "5-А",
            Subjects = new List<Subject>
            {
                new() { Id = MathId, Name = "Математика" },
                new() { Id = HistoryId, Name = "История" }
            }
        });
        _studentRepositoryMock.Setup(r => r.GetByClassIdAsync(ClassId)).ReturnsAsync(new List<Student>
        {
            new() { Id = 1, LastName = "Иванов", FirstName = "Иван", SchoolClassId = ClassId },
            new() { Id = 2, LastName = "Петров", FirstName = "Пётр", SchoolClassId = ClassId }
        });
    }

    private void SetupStats(params GradeStat[] stats)
    {
        _gradeRepositoryMock
            .Setup(r => r.GetStatsForClassAsync(ClassId, Term.StartDate, Term.EndDate))
            .ReturnsAsync(stats.ToList());
    }

    [Fact]
    public async Task BuildAsync_MissingTerm_ReturnsNull()
    {
        var report = await _service.BuildAsync(99, ClassId);

        Assert.Null(report);
    }

    [Fact]
    public async Task BuildAsync_MissingClass_ReturnsNull()
    {
        var report = await _service.BuildAsync(TermId, 99);

        Assert.Null(report);
    }

    [Fact]
    public async Task BuildAsync_ClassWithoutStudents_ReturnsEmptyReport()
    {
        _studentRepositoryMock.Setup(r => r.GetByClassIdAsync(ClassId)).ReturnsAsync(new List<Student>());
        SetupStats();

        var report = await _service.BuildAsync(TermId, ClassId);

        Assert.NotNull(report);
        Assert.Empty(report.Rows);
        Assert.False(report.HasGrades);
    }

    [Fact]
    public async Task BuildAsync_TermWithoutGrades_HasNoAveragesAndNoNaN()
    {
        SetupStats();

        var report = await _service.BuildAsync(TermId, ClassId);

        Assert.NotNull(report);
        Assert.False(report.HasGrades);
        Assert.Equal(2, report.Rows.Count);
        Assert.All(report.Rows, row =>
        {
            Assert.Null(row.Average);
            Assert.Null(row.FinalGrade);
            Assert.All(row.SubjectAverages.Values, value => Assert.Null(value));
        });
        Assert.All(report.SubjectAverages.Values, value => Assert.Null(value));
    }

    [Fact]
    public async Task BuildAsync_SubjectsAreSortedByName()
    {
        SetupStats();

        var report = await _service.BuildAsync(TermId, ClassId);

        Assert.NotNull(report);
        Assert.Equal(new[] { "История", "Математика" }, report.Subjects.Select(s => s.Name).ToArray());
    }

    [Fact]
    public async Task BuildAsync_CalculatesCellsStudentAverageAndFinalGrade()
    {
        SetupStats(
            new GradeStat(1, MathId, 17, 2),
            new GradeStat(1, HistoryId, 17, 2),
            new GradeStat(2, MathId, 5, 1));

        var report = await _service.BuildAsync(TermId, ClassId);

        Assert.NotNull(report);
        Assert.True(report.HasGrades);

        var ivanov = report.Rows[0];
        Assert.Equal("Иванов Иван", ivanov.StudentName);
        Assert.Equal(8.5, ivanov.SubjectAverages[MathId]);
        Assert.Equal(8.5, ivanov.SubjectAverages[HistoryId]);
        Assert.Equal(8.5, ivanov.Average);
        Assert.Equal(9, ivanov.FinalGrade);

        var petrov = report.Rows[1];
        Assert.Equal(5.0, petrov.SubjectAverages[MathId]);
        Assert.Null(petrov.SubjectAverages[HistoryId]);
        Assert.Equal(5.0, petrov.Average);
        Assert.Equal(5, petrov.FinalGrade);
    }

    [Fact]
    public async Task BuildAsync_StudentAverageUsesAllGradesNotAverageOfAverages()
    {
        SetupStats(
            new GradeStat(1, MathId, 30, 3),
            new GradeStat(1, HistoryId, 6, 1));

        var report = await _service.BuildAsync(TermId, ClassId);

        Assert.NotNull(report);
        Assert.Equal(9.0, report.Rows[0].Average);
        Assert.Equal(9, report.Rows[0].FinalGrade);
    }

    [Fact]
    public async Task BuildAsync_ClassRowUsesAllGradesOfSubject()
    {
        SetupStats(
            new GradeStat(1, MathId, 17, 2),
            new GradeStat(2, MathId, 5, 1));

        var report = await _service.BuildAsync(TermId, ClassId);

        Assert.NotNull(report);
        Assert.Equal(22.0 / 3, report.SubjectAverages[MathId]);
        Assert.Null(report.SubjectAverages[HistoryId]);
    }
}
