using Microsoft.EntityFrameworkCore;
using School.Data.Contracts.Exceptions;
using School.Data.Repositories;
using School.Domain;

namespace School.Tests.Integration;

public class AcademicTermRepositoryTests : SqliteTestBase
{
    private static readonly DateTime FirstTermStart = new(2026, 9, 1);
    private static readonly DateTime FirstTermEnd = new(2026, 10, 31);

    private readonly AcademicTermRepository _repository;

    public AcademicTermRepositoryTests()
    {
        _repository = new AcademicTermRepository(ContextFactory);
    }

    [Fact]
    public async Task HasOverlapAsync_OverlappingRange_ReturnsTrue()
    {
        await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var overlaps = await _repository.HasOverlapAsync(new DateTime(2026, 10, 15), new DateTime(2026, 12, 20));

        Assert.True(overlaps);
    }

    [Fact]
    public async Task HasOverlapAsync_RangeContainingExistingTerm_ReturnsTrue()
    {
        await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var overlaps = await _repository.HasOverlapAsync(new DateTime(2026, 8, 1), new DateTime(2026, 11, 30));

        Assert.True(overlaps);
    }

    [Fact]
    public async Task HasOverlapAsync_SharedBoundaryDay_ReturnsTrue()
    {
        await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var overlaps = await _repository.HasOverlapAsync(FirstTermEnd, new DateTime(2026, 12, 20));

        Assert.True(overlaps);
    }

    [Fact]
    public async Task HasOverlapAsync_AdjacentRange_ReturnsFalse()
    {
        await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var after = await _repository.HasOverlapAsync(new DateTime(2026, 11, 1), new DateTime(2026, 12, 20));
        var before = await _repository.HasOverlapAsync(new DateTime(2026, 6, 1), new DateTime(2026, 8, 31));

        Assert.False(after);
        Assert.False(before);
    }

    [Fact]
    public async Task HasOverlapAsync_ExcludeId_DoesNotConflictWithItself()
    {
        var term = await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var overlaps = await _repository.HasOverlapAsync(FirstTermStart, new DateTime(2026, 11, 5), term.Id);

        Assert.False(overlaps);
    }

    [Fact]
    public async Task GetByDateAsync_FirstAndLastDay_ReturnTerm()
    {
        var term = await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var first = await _repository.GetByDateAsync(FirstTermStart);
        var last = await _repository.GetByDateAsync(FirstTermEnd);

        Assert.Equal(term.Id, first?.Id);
        Assert.Equal(term.Id, last?.Id);
    }

    [Fact]
    public async Task GetByDateAsync_DateOutsideTerms_ReturnsNull()
    {
        await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var before = await _repository.GetByDateAsync(new DateTime(2026, 8, 31));
        var after = await _repository.GetByDateAsync(new DateTime(2026, 11, 1));

        Assert.Null(before);
        Assert.Null(after);
    }

    [Fact]
    public async Task GetAllWithGradeCountsAsync_CountsGradesInsideTermIncludingBoundaries()
    {
        await SeedTermAsync("II четверть", new DateTime(2026, 11, 1), new DateTime(2026, 12, 25));
        await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);
        var subject = await SeedSubjectAsync("Математика");
        await SeedGradeAsync(student.Id, subject.Id, 8, FirstTermStart);
        await SeedGradeAsync(student.Id, subject.Id, 7, FirstTermEnd);
        await SeedGradeAsync(student.Id, subject.Id, 9, new DateTime(2026, 8, 31));

        var terms = await _repository.GetAllWithGradeCountsAsync();

        Assert.Equal(2, terms.Count);
        Assert.Equal("I четверть", terms[0].Term.Name);
        Assert.Equal(2, terms[0].GradeCount);
        Assert.Equal(0, terms[1].GradeCount);
    }

    [Fact]
    public async Task HasGradesAsync_ReturnsWhetherRangeContainsGrades()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);
        var subject = await SeedSubjectAsync("Математика");
        await SeedGradeAsync(student.Id, subject.Id, 8, FirstTermEnd);

        Assert.True(await _repository.HasGradesAsync(FirstTermStart, FirstTermEnd));
        Assert.False(await _repository.HasGradesAsync(new DateTime(2026, 11, 1), new DateTime(2026, 12, 25)));
    }

    [Fact]
    public async Task AddAsync_DuplicateName_ThrowsDuplicateNameException()
    {
        await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        await Assert.ThrowsAsync<DuplicateNameException>(() => _repository.AddAsync(new AcademicTerm
        {
            Name = "I четверть",
            StartDate = new DateTime(2027, 9, 1),
            EndDate = new DateTime(2027, 10, 31)
        }));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAndMissing()
    {
        var term = await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var existing = await _repository.GetByIdAsync(term.Id);
        var missing = await _repository.GetByIdAsync(999);

        Assert.NotNull(existing);
        Assert.Equal("I четверть", existing.Name);
        Assert.Null(missing);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTerm_ChangesNameAndDatesButNotStatus()
    {
        var term = await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var updated = await _repository.UpdateAsync(new AcademicTerm
        {
            Id = term.Id,
            Name = "Первая четверть",
            StartDate = new DateTime(2026, 9, 2),
            EndDate = new DateTime(2026, 11, 2),
            IsClosed = true
        });

        Assert.True(updated);
        await using var context = CreateContext();
        var saved = await context.AcademicTerms.SingleAsync(t => t.Id == term.Id);
        Assert.Equal("Первая четверть", saved.Name);
        Assert.Equal(new DateTime(2026, 9, 2), saved.StartDate);
        Assert.Equal(new DateTime(2026, 11, 2), saved.EndDate);
        Assert.False(saved.IsClosed);
    }

    [Fact]
    public async Task UpdateAsync_MissingTerm_ReturnsFalse()
    {
        var updated = await _repository.UpdateAsync(new AcademicTerm { Id = 999, Name = "X", StartDate = FirstTermStart, EndDate = FirstTermEnd });

        Assert.False(updated);
    }

    [Fact]
    public async Task SetClosedAsync_ExistingTerm_ChangesStatus()
    {
        var term = await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var closed = await _repository.SetClosedAsync(term.Id, true);

        Assert.True(closed);
        await using var context = CreateContext();
        var saved = await context.AcademicTerms.SingleAsync(t => t.Id == term.Id);
        Assert.True(saved.IsClosed);
    }

    [Fact]
    public async Task SetClosedAsync_MissingTerm_ReturnsFalse()
    {
        var closed = await _repository.SetClosedAsync(999, true);

        Assert.False(closed);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTerm_RemovesTerm()
    {
        var term = await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        var deleted = await _repository.DeleteAsync(term.Id);

        Assert.True(deleted);
        await using var context = CreateContext();
        Assert.False(await context.AcademicTerms.AnyAsync());
    }

    [Fact]
    public async Task DeleteAsync_MissingTerm_ReturnsFalse()
    {
        var deleted = await _repository.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ExistsByNameAsync_ExcludeId_DoesNotCountRecordAsItsOwnDuplicate()
    {
        var term = await SeedTermAsync("I четверть", FirstTermStart, FirstTermEnd);

        Assert.True(await _repository.ExistsByNameAsync("I четверть"));
        Assert.False(await _repository.ExistsByNameAsync("I четверть", term.Id));
    }

    private async Task<AcademicTerm> SeedTermAsync(string name, DateTime startDate, DateTime endDate)
    {
        await using var context = CreateContext();
        var term = new AcademicTerm { Name = name, StartDate = startDate, EndDate = endDate };
        context.AcademicTerms.Add(term);
        await context.SaveChangesAsync();
        return term;
    }
}
