using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Services;

namespace School.Tests;

public class GradeServiceTests
{
    private static readonly DateTime Today = DateTime.Today;
    private static readonly DateTime ClosedTermDay = DateTime.Today.AddDays(-40);

    private readonly Mock<IGradeRepository> _gradeRepositoryMock;
    private readonly Mock<IStudentRepository> _studentRepositoryMock;
    private readonly Mock<IAcademicTermRepository> _termRepositoryMock;
    private readonly GradeService _service;

    public GradeServiceTests()
    {
        _gradeRepositoryMock = new Mock<IGradeRepository>();
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _termRepositoryMock = new Mock<IAcademicTermRepository>();
        _service = new GradeService(
            _gradeRepositoryMock.Object,
            _studentRepositoryMock.Object,
            _termRepositoryMock.Object,
            NullLogger<GradeService>.Instance);

        _studentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeStudentWithSubject(1));
        _termRepositoryMock
            .Setup(r => r.GetByDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new AcademicTerm { Id = 1, Name = "I четверть", StartDate = Today.AddDays(-10), EndDate = Today.AddDays(10) });
        _termRepositoryMock
            .Setup(r => r.GetByDateAsync(ClosedTermDay))
            .ReturnsAsync(new AcademicTerm { Id = 2, Name = "Закрытая четверть", StartDate = ClosedTermDay, EndDate = ClosedTermDay.AddDays(5), IsClosed = true });
    }

    private static Student MakeStudentWithSubject(int subjectId)
    {
        var subject = new Subject { Id = subjectId, Name = "Математика" };
        return new Student
        {
            Id = 1,
            SchoolClass = new SchoolClass
            {
                Id = 1,
                Name = "5-А",
                Subjects = new List<Subject> { subject }
            }
        };
    }

    private static Grade MakeGrade(int id = 0, int value = 8, DateTime? date = null) => new()
    {
        Id = id,
        StudentId = 1,
        SubjectId = 1,
        Value = value,
        Date = date ?? Today
    };

    [Fact]
    public async Task GetByStudentIdAsync_ReturnsRepositoryData()
    {
        var grades = new List<Grade> { MakeGrade(1) };
        _gradeRepositoryMock.Setup(r => r.GetByStudentIdAsync(1)).ReturnsAsync(grades);

        var result = await _service.GetByStudentIdAsync(1);

        Assert.Same(grades, result);
    }

    [Fact]
    public async Task GetAverageGradeAsync_NoGrades_ReturnsNull()
    {
        _gradeRepositoryMock
            .Setup(r => r.GetByStudentIdAsync(1))
            .ReturnsAsync(new List<Grade>());

        var result = await _service.GetAverageGradeAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAverageGradeAsync_WithGrades_ReturnsCorrectAverage()
    {
        var grades = new List<Grade>
        {
            new Grade { Value = 8 },
            new Grade { Value = 10 },
            new Grade { Value = 6 }
        };
        _gradeRepositoryMock
            .Setup(r => r.GetByStudentIdAsync(1))
            .ReturnsAsync(grades);

        var result = await _service.GetAverageGradeAsync(1);

        Assert.Equal(8, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task AddAsync_InvalidValue_ThrowsArgumentException(int value)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(MakeGrade(value: value)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task AddAsync_ValidValue_CallsRepository(int value)
    {
        var grade = MakeGrade(value: value);

        await _service.AddAsync(grade);

        _gradeRepositoryMock.Verify(r => r.AddAsync(grade), Times.Once);
    }

    [Fact]
    public async Task AddAsync_DateWithTime_IsSavedAsDay()
    {
        var grade = MakeGrade(date: Today.AddHours(15));

        await _service.AddAsync(grade);

        Assert.Equal(Today, grade.Date);
    }

    [Fact]
    public async Task AddAsync_Tomorrow_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(MakeGrade(date: Today.AddDays(1))));
    }

    [Fact]
    public async Task AddAsync_SubjectNotAssigned_ThrowsArgumentException()
    {
        var grade = MakeGrade();
        grade.SubjectId = 99;

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(grade));
    }

    [Fact]
    public async Task AddAsync_MissingStudent_ThrowsArgumentException()
    {
        var grade = MakeGrade();
        grade.StudentId = 2;

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(grade));
    }

    [Fact]
    public async Task AddAsync_DateOutsideTerms_ThrowsArgumentException()
    {
        _termRepositoryMock.Setup(r => r.GetByDateAsync(Today)).ReturnsAsync((AcademicTerm?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(MakeGrade()));

        _gradeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Grade>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ClosedTerm_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(MakeGrade(date: ClosedTermDay)));

        _gradeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Grade>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MissingGrade_ReturnsFalse()
    {
        _gradeRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Grade?)null);

        var result = await _service.UpdateAsync(MakeGrade(5));

        Assert.False(result);
        _gradeRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Grade>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_GradeFromClosedTerm_ThrowsArgumentException()
    {
        _gradeRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(MakeGrade(5, date: ClosedTermDay));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(MakeGrade(5)));

        _gradeRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Grade>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MoveIntoClosedTerm_ThrowsArgumentException()
    {
        _gradeRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(MakeGrade(5));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(MakeGrade(5, date: ClosedTermDay)));
    }

    [Fact]
    public async Task UpdateAsync_OpenTerm_UpdatesGrade()
    {
        var grade = MakeGrade(5, value: 9);
        _gradeRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(MakeGrade(5));
        _gradeRepositoryMock.Setup(r => r.UpdateAsync(grade)).ReturnsAsync(true);

        var result = await _service.UpdateAsync(grade);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_MissingGrade_ReturnsFalse()
    {
        _gradeRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Grade?)null);

        var result = await _service.DeleteAsync(5);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_GradeFromClosedTerm_ThrowsArgumentException()
    {
        _gradeRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(MakeGrade(5, date: ClosedTermDay));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(5));

        _gradeRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_GradeOutsideAnyTerm_Deletes()
    {
        var oldDate = new DateTime(2020, 5, 5);
        _gradeRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(MakeGrade(5, date: oldDate));
        _termRepositoryMock.Setup(r => r.GetByDateAsync(oldDate)).ReturnsAsync((AcademicTerm?)null);
        _gradeRepositoryMock.Setup(r => r.DeleteAsync(5)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(5);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_GradeFromOpenTerm_Deletes()
    {
        _gradeRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(MakeGrade(5));
        _gradeRepositoryMock.Setup(r => r.DeleteAsync(5)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(5);

        Assert.True(result);
    }
}
