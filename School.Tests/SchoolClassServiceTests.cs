using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Contracts.Exceptions;
using School.Data.Contracts.Repositories;
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
        var schoolClass = new SchoolClass { Name = "5-А" };

        await _service.AddAsync(schoolClass);

        _repositoryMock.Verify(r => r.AddAsync(schoolClass), Times.Once);
    }

    [Fact]
    public async Task AddAsync_DuplicateName_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.ExistsByNameAsync("5-А", It.IsAny<int?>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(new SchoolClass { Name = "5-А" }));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SchoolClass>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_UniqueIndexViolation_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SchoolClass>()))
            .ThrowsAsync(new DuplicateNameException("duplicate"));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(new SchoolClass { Name = "5-А" }));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.ExistsByNameAsync("5-Б", 3))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(new SchoolClass { Id = 3, Name = "5-Б" }));

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SchoolClass>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MissingRecord_ReturnsFalse()
    {
        var schoolClass = new SchoolClass { Id = 9, Name = "5-А" };
        _repositoryMock.Setup(r => r.UpdateAsync(schoolClass)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(schoolClass);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_UniqueIndexViolation_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SchoolClass>()))
            .ThrowsAsync(new DuplicateNameException("duplicate"));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(new SchoolClass { Id = 3, Name = "5-Б" }));
    }

    [Fact]
    public async Task DeleteAsync_ClassHasStudents_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.DeleteAsync(4))
            .ThrowsAsync(new EntityInUseException("in use"));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(4));
    }

    [Fact]
    public async Task DeleteAsync_ExistingClass_ReturnsTrue()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(4)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(4);

        Assert.True(result);
    }

    [Fact]
    public async Task RemoveSubjectAsync_NotAssigned_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.RemoveSubjectAsync(1, 2)).ReturnsAsync(false);

        var result = await _service.RemoveSubjectAsync(1, 2);

        Assert.False(result);
    }

    [Fact]
    public async Task AssignSubjectAsync_MissingClass_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.AddSubjectAsync(1, 2)).ReturnsAsync(false);

        var result = await _service.AssignSubjectAsync(1, 2);

        Assert.False(result);
    }

    [Fact]
    public async Task AssignSubjectAsync_Existing_ReturnsTrue()
    {
        _repositoryMock.Setup(r => r.AddSubjectAsync(1, 2)).ReturnsAsync(true);

        var result = await _service.AssignSubjectAsync(1, 2);

        Assert.True(result);
    }

    [Theory]
    [InlineData("1-А")]
    [InlineData("9-Б")]
    [InlineData("11-В")]
    [InlineData("12-А")]
    public async Task AddAsync_ValidFormat_CallsRepository(string name)
    {
        var schoolClass = new SchoolClass { Name = name };

        await _service.AddAsync(schoolClass);

        _repositoryMock.Verify(r => r.AddAsync(schoolClass), Times.Once);
    }

    [Theory]
    [InlineData("0-А")]
    [InlineData("13-А")]
    [InlineData("5А")]
    [InlineData("5-а")]
    [InlineData("5-АБ")]
    [InlineData("5-A")]
    public async Task AddAsync_InvalidFormat_ThrowsArgumentException(string name)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(new SchoolClass { Name = name }));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SchoolClass>()), Times.Never);
    }
}