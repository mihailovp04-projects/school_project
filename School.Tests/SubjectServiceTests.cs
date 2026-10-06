using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Repositories;
using School.Domain;
using School.Services.Services;

namespace School.Tests;

public class SubjectServiceTests
{
    private readonly Mock<ISubjectRepository> _repositoryMock;
    private readonly SubjectService _service;

    public SubjectServiceTests()
    {
        _repositoryMock = new Mock<ISubjectRepository>();
        _service = new SubjectService(_repositoryMock.Object, NullLogger<SubjectService>.Instance);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddAsync_EmptyName_ThrowsArgumentException(string? name)
    {
        var subject = new Subject { Name = name! };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(subject));
    }

    [Fact]
    public async Task AddAsync_ValidName_CallsRepository()
    {
        var subject = new Subject { Name = "Mathematics" };

        await _service.AddAsync(subject);

        _repositoryMock.Verify(r => r.AddAsync(subject), Times.Once);
    }

    [Fact]
    public async Task AddAsync_DuplicateName_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.ExistsByNameAsync("Mathematics", It.IsAny<int?>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(new Subject { Name = "Mathematics" }));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subject>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_UniqueIndexViolation_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Subject>()))
            .ThrowsAsync(new DbUpdateException("duplicate"));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(new Subject { Name = "Mathematics" }));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.ExistsByNameAsync("Physics", 2))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(new Subject { Id = 2, Name = "Physics" }));

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Subject>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MissingRecord_ReturnsFalse()
    {
        var subject = new Subject { Id = 5, Name = "Physics" };
        _repositoryMock.Setup(r => r.UpdateAsync(subject)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(subject);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_MissingRecord_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(7)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(7);

        Assert.False(result);
    }
}