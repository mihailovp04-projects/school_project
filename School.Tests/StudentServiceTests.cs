using Moq;
using School.Data.Repositories;
using School.Domain;
using School.Services.Services;
using Microsoft.Extensions.Logging.Abstractions;

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
        var student = new Student { FirstName = "", LastName = "Ivanov", BirthDate = new DateTime(2010, 1, 1) };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(student));
    }

    [Fact]
    public async Task AddAsync_EmptyLastName_ThrowsArgumentException()
    {
        var student = new Student { FirstName = "Ivan", LastName = "   ", BirthDate = new DateTime(2010, 1, 1) };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(student));
    }

    [Fact]
    public async Task AddAsync_FutureBirthDate_ThrowsArgumentException()
    {
        var student = new Student
        {
            FirstName = "Ivan",
            LastName = "Ivanov",
            BirthDate = DateTime.Now.AddYears(1)
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
            BirthDate = new DateTime(2010, 1, 1)
        };

        await _service.AddAsync(student);

        _repositoryMock.Verify(r => r.AddAsync(student), Times.Once);
    }
}