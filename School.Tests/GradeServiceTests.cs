using Moq;
using School.Data.Repositories;
using School.Domain;
using School.Services.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace School.Tests;

public class GradeServiceTests
{
    private readonly Mock<IGradeRepository> _repositoryMock;
    private readonly GradeService _service;

    public GradeServiceTests()
    {
        _repositoryMock = new Mock<IGradeRepository>();
        _service = new GradeService(_repositoryMock.Object, NullLogger<GradeService>.Instance);
    }

    [Fact]
    public async Task GetAverageGradeAsync_NoGrades_ReturnsZero()
    {
        _repositoryMock
            .Setup(r => r.GetByStudentIdAsync(1))
            .ReturnsAsync(new List<Grade>());

        var result = await _service.GetAverageGradeAsync(1);

        Assert.Equal(0, result);
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
        _repositoryMock
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

        var grade = new Grade { Value = value };
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(grade));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task AddAsync_ValidValue_CallsRepository(int value)
    {
        var grade = new Grade { Value = value };

        await _service.AddAsync(grade);

        _repositoryMock.Verify(r => r.AddAsync(grade), Times.Once);
    }
}