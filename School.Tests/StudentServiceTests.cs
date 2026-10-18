using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Contracts.Exceptions;
using School.Data.Contracts.Models;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Services;

namespace School.Tests;

public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _repositoryMock;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _repositoryMock = new Mock<IStudentRepository>();
        _service = new StudentService(_repositoryMock.Object, NullLogger<StudentService>.Instance);
    }

    [Fact]
    public async Task AddAsync_EmptyFirstName_ThrowsArgumentException()
    {
        var student = new Student { FirstName = "", LastName = "Ivanov", BirthDate = new DateTime(2010, 1, 1), SchoolClassId = 1 };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(student));
    }

    [Fact]
    public async Task AddAsync_EmptyLastName_ThrowsArgumentException()
    {
        var student = new Student { FirstName = "Ivan", LastName = "   ", BirthDate = new DateTime(2010, 1, 1), SchoolClassId = 1 };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(student));
    }

    [Fact]
    public async Task AddAsync_FutureBirthDate_ThrowsArgumentException()
    {
        var student = new Student
        {
            FirstName = "Ivan",
            LastName = "Ivanov",
            BirthDate = DateTime.UtcNow.AddYears(1),
            SchoolClassId = 1
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(student));
    }

    [Fact]
    public async Task AddAsync_NoClassSelected_ThrowsArgumentException()
    {
        var student = new Student
        {
            FirstName = "Ivan",
            LastName = "Ivanov",
            BirthDate = new DateTime(2010, 1, 1),
            SchoolClassId = 0
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(student));
    }

    [Fact]
    public async Task AddAsync_ValidStudent_CallsRepository()
    {
        var student = new Student
        {
            FirstName = "Ivan",
            LastName = "Ivanov",
            BirthDate = new DateTime(2010, 1, 1),
            SchoolClassId = 1
        };

        await _service.AddAsync(student);

        _repositoryMock.Verify(r => r.AddAsync(student), Times.Once);
    }

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-3, 500, 1, 100)]
    [InlineData(2, 20, 2, 20)]
    public async Task GetPagedAsync_NormalizesPageAndPageSize(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var filter = new StudentFilter();
        _repositoryMock
            .Setup(r => r.GetPagedAsync(filter, expectedPage, expectedPageSize))
            .ReturnsAsync(new PagedResult<Student>(new List<Student> { new() }, 1, expectedPage, expectedPageSize));

        var result = await _service.GetPagedAsync(filter, page, pageSize);

        Assert.Equal(expectedPage, result.Page);
        _repositoryMock.Verify(r => r.GetPagedAsync(filter, expectedPage, expectedPageSize), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_PageAfterLast_ReturnsLastPage()
    {
        var filter = new StudentFilter();
        _repositoryMock
            .Setup(r => r.GetPagedAsync(filter, 5, 2))
            .ReturnsAsync(new PagedResult<Student>(new List<Student>(), 3, 5, 2));
        _repositoryMock
            .Setup(r => r.GetPagedAsync(filter, 2, 2))
            .ReturnsAsync(new PagedResult<Student>(new List<Student> { new() }, 3, 2, 2));

        var result = await _service.GetPagedAsync(filter, 5, 2);

        Assert.Equal(2, result.Page);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetPagedAsync_NoStudents_DoesNotRequestAnotherPage()
    {
        var filter = new StudentFilter();
        _repositoryMock
            .Setup(r => r.GetPagedAsync(filter, 1, 20))
            .ReturnsAsync(new PagedResult<Student>(new List<Student>(), 0, 1, 20));

        var result = await _service.GetPagedAsync(filter, 1, 20);

        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        _repositoryMock.Verify(r => r.GetPagedAsync(It.IsAny<StudentFilter>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_ReturnsRepositoryCount()
    {
        _repositoryMock.Setup(r => r.CountAsync()).ReturnsAsync(42);

        Assert.Equal(42, await _service.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_StudentHasGrades_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.DeleteAsync(5))
            .ThrowsAsync(new EntityInUseException("in use"));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(5));
    }

    [Fact]
    public async Task DeleteAsync_MissingRecord_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(5)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(5);

        Assert.False(result);
    }
}