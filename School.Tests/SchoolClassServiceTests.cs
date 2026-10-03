using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Repositories;
using School.Domain;
using School.Services.Services;

namespace School.Tests;

public class SchoolClassServiceTests
{
    private readonly Mock<ISchoolClassRepository> _repositoryMock;
    private readonly SchoolClassService _service;

    public SchoolClassServiceTests()
    {
        _repositoryMock = new Mock<ISchoolClassRepository>();
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<SchoolClass>());
        _service = new SchoolClassService(_repositoryMock.Object, NullLogger<SchoolClassService>.Instance);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddAsync_EmptyName_ThrowsArgumentException(string? name)
    {
        var schoolClass = new SchoolClass { Name = name! };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(schoolClass));
    }

    [Fact]
    public async Task AddAsync_ValidName_CallsRepository()
    {
        var schoolClass = new SchoolClass { Name = "5-A" };

        await _service.AddAsync(schoolClass);

        _repositoryMock.Verify(r => r.AddAsync(schoolClass), Times.Once);
    }
}