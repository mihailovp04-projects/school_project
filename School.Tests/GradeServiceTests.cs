using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Repositories;
using School.Domain;
using School.Services.Services;

namespace School.Tests;

public class GradeServiceTests
{
    private readonly Mock<IGradeRepository> _gradeRepositoryMock;
    private readonly Mock<IStudentRepository> _studentRepositoryMock;
    private readonly GradeService _service;

    public GradeServiceTests()
    {
        _gradeRepositoryMock = new Mock<IGradeRepository>();
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _service = new GradeService(_gradeRepositoryMock.Object, _studentRepositoryMock.Object, NullLogger<GradeService>.Instance);
    }

    private Student MakeStudentWithSubject(int subjectId)
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
        _studentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeStudentWithSubject(1));
        var grade = new Grade { StudentId = 1, SubjectId = 1, Value = value, Date = DateTime.Now };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(grade));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task AddAsync_ValidValue_CallsRepository(int value)
    {
        _studentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeStudentWithSubject(1));
        var grade = new Grade { StudentId = 1, SubjectId = 1, Value = value, Date = DateTime.Now };

        await _service.AddAsync(grade);

        _gradeRepositoryMock.Verify(r => r.AddAsync(grade), Times.Once);
    }

    [Fact]
    public async Task AddAsync_SubjectNotAssignedToClass_ThrowsArgumentException()
    {
        _studentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeStudentWithSubject(1));
        var grade = new Grade { StudentId = 1, SubjectId = 99, Value = 8, Date = DateTime.Now };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(grade));
    }
}