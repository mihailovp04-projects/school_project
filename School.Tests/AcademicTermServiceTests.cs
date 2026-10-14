using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Contracts.Exceptions;
using School.Data.Contracts.Models;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Services;

namespace School.Tests;

public class AcademicTermServiceTests
{
    private readonly Mock<IAcademicTermRepository> _repositoryMock;
    private readonly AcademicTermService _service;

    public AcademicTermServiceTests()
    {
        _repositoryMock = new Mock<IAcademicTermRepository>();
        _service = new AcademicTermService(_repositoryMock.Object, NullLogger<AcademicTermService>.Instance);
    }

    private static AcademicTerm MakeTerm(int id = 0, bool isClosed = false) => new()
    {
        Id = id,
        Name = "I четверть",
        StartDate = new DateTime(2026, 9, 1),
        EndDate = new DateTime(2026, 10, 31),
        IsClosed = isClosed
    };

    [Fact]
    public async Task GetAllAsync_ReturnsRepositoryData()
    {
        var summaries = new List<AcademicTermSummary> { new(MakeTerm(1), 5) };
        _repositoryMock.Setup(r => r.GetAllWithGradeCountsAsync()).ReturnsAsync(summaries);

        var result = await _service.GetAllAsync();

        Assert.Same(summaries, result);
    }

    [Fact]
    public async Task AddAsync_ValidTerm_NormalizesAndCallsRepository()
    {
        var term = new AcademicTerm
        {
            Name = "  I четверть  ",
            StartDate = new DateTime(2026, 9, 1, 14, 30, 0),
            EndDate = new DateTime(2026, 10, 31, 8, 0, 0)
        };

        await _service.AddAsync(term);

        Assert.Equal("I четверть", term.Name);
        Assert.Equal(new DateTime(2026, 9, 1), term.StartDate);
        Assert.Equal(new DateTime(2026, 10, 31), term.EndDate);
        _repositoryMock.Verify(r => r.AddAsync(term), Times.Once);
    }

    [Fact]
    public async Task AddAsync_EndBeforeStart_ThrowsArgumentException()
    {
        var term = MakeTerm();
        term.EndDate = new DateTime(2026, 8, 31);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(term));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<AcademicTerm>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_EndEqualsStart_ThrowsArgumentException()
    {
        var term = MakeTerm();
        term.EndDate = term.StartDate;

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(term));
    }

    [Fact]
    public async Task AddAsync_MissingDates_ThrowsArgumentException()
    {
        var term = new AcademicTerm { Name = "I четверть" };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(term));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_EmptyName_ThrowsArgumentException(string name)
    {
        var term = MakeTerm();
        term.Name = name;

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(term));
    }

    [Fact]
    public async Task AddAsync_DuplicateName_ThrowsArgumentException()
    {
        _repositoryMock.Setup(r => r.ExistsByNameAsync("I четверть", null)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(MakeTerm()));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<AcademicTerm>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_OverlappingTerm_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.HasOverlapAsync(new DateTime(2026, 9, 1), new DateTime(2026, 10, 31), null))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(MakeTerm()));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<AcademicTerm>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_UniqueIndexViolation_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<AcademicTerm>()))
            .ThrowsAsync(new DuplicateNameException("duplicate"));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(MakeTerm()));
    }

    [Fact]
    public async Task UpdateAsync_MissingTerm_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((AcademicTerm?)null);

        var result = await _service.UpdateAsync(MakeTerm(3));

        Assert.False(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AcademicTerm>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ClosedTerm_ThrowsArgumentException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(MakeTerm(3, isClosed: true));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(MakeTerm(3)));

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AcademicTerm>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OverlapWithAnotherTerm_ThrowsArgumentException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(MakeTerm(3));
        _repositoryMock
            .Setup(r => r.HasOverlapAsync(new DateTime(2026, 9, 1), new DateTime(2026, 10, 31), 3))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(MakeTerm(3)));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsArgumentException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(MakeTerm(3));
        _repositoryMock.Setup(r => r.ExistsByNameAsync("I четверть", 3)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(MakeTerm(3)));
    }

    [Fact]
    public async Task UpdateAsync_UniqueIndexViolation_ThrowsArgumentException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(MakeTerm(3));
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<AcademicTerm>()))
            .ThrowsAsync(new DuplicateNameException("duplicate"));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(MakeTerm(3)));
    }

    [Fact]
    public async Task UpdateAsync_OpenTerm_ChecksOverlapWithoutItselfAndUpdates()
    {
        var term = MakeTerm(3);
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(MakeTerm(3));
        _repositoryMock.Setup(r => r.UpdateAsync(term)).ReturnsAsync(true);

        var result = await _service.UpdateAsync(term);

        Assert.True(result);
        _repositoryMock.Verify(r => r.HasOverlapAsync(term.StartDate, term.EndDate, 3), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetClosedAsync_PassesStatusToRepository(bool isClosed)
    {
        _repositoryMock.Setup(r => r.SetClosedAsync(3, isClosed)).ReturnsAsync(true);

        var result = await _service.SetClosedAsync(3, isClosed);

        Assert.True(result);
    }

    [Fact]
    public async Task SetClosedAsync_MissingTerm_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.SetClosedAsync(3, true)).ReturnsAsync(false);

        var result = await _service.SetClosedAsync(3, true);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_MissingTerm_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((AcademicTerm?)null);

        var result = await _service.DeleteAsync(3);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ClosedTerm_ThrowsArgumentException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(MakeTerm(3, isClosed: true));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(3));

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_TermWithGrades_ThrowsArgumentException()
    {
        var term = MakeTerm(3);
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(term);
        _repositoryMock.Setup(r => r.HasGradesAsync(term.StartDate, term.EndDate)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(3));

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_EmptyOpenTerm_Deletes()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(MakeTerm(3));
        _repositoryMock.Setup(r => r.DeleteAsync(3)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(3);

        Assert.True(result);
    }
}
