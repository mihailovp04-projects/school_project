using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Contracts.Exceptions;
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