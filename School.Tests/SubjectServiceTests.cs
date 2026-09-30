using Moq;
using School.Data.Repositories;
using School.Domain;
using School.Services.Services;
using Microsoft.Extensions.Logging.Abstractions;

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
}