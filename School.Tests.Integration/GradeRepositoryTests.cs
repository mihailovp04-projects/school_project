using Microsoft.EntityFrameworkCore;
using School.Data.Repositories;
using School.Domain;

namespace School.Tests.Integration;

public class GradeRepositoryTests : SqliteTestBase
{
    private readonly GradeRepository _repository;

    public GradeRepositoryTests()
    {
        _repository = new GradeRepository(ContextFactory);
    }

    [Fact]
    public async Task GetByStudentIdAsync_ReturnsOnlyThatStudentsGradesWithSubject()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var first = await SeedStudentAsync(schoolClass.Id, "Иванов");
        var second = await SeedStudentAsync(schoolClass.Id, "Петров");
        var subject = await SeedSubjectAsync("Математика");
        await SeedGradeAsync(first.Id, subject.Id, 9);
        await SeedGradeAsync(first.Id, subject.Id, 7);
        await SeedGradeAsync(second.Id, subject.Id, 4);

        var grades = await _repository.GetByStudentIdAsync(first.Id);

        Assert.Equal(2, grades.Count);
        Assert.All(grades, g => Assert.Equal("Математика", g.Subject.Name));
    }

    [Fact]
    public async Task GetStatsForClassAsync_GroupsByStudentAndSubjectWithinPeriod()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var otherClass = await SeedClassAsync("6-Б");
        var ivanov = await SeedStudentAsync(schoolClass.Id, "Иванов");
        var petrov = await SeedStudentAsync(schoolClass.Id, "Петров");
        var stranger = await SeedStudentAsync(otherClass.Id, "Сидоров");
        var math = await SeedSubjectAsync("Математика");
        var history = await SeedSubjectAsync("История");
        var start = new DateTime(2026, 9, 1);
        var end = new DateTime(2026, 10, 31);
        await SeedGradeAsync(ivanov.Id, math.Id, 8, start);
        await SeedGradeAsync(ivanov.Id, math.Id, 9, end);
        await SeedGradeAsync(ivanov.Id, history.Id, 7, new DateTime(2026, 10, 10));
        await SeedGradeAsync(petrov.Id, math.Id, 5, new DateTime(2026, 9, 20));
        await SeedGradeAsync(ivanov.Id, math.Id, 2, new DateTime(2026, 11, 1));
        await SeedGradeAsync(stranger.Id, math.Id, 10, new DateTime(2026, 9, 20));

        var stats = await _repository.GetStatsForClassAsync(schoolClass.Id, start, end);

        Assert.Equal(3, stats.Count);
        var ivanovMath = Assert.Single(stats, s => s.StudentId == ivanov.Id && s.SubjectId == math.Id);
        Assert.Equal(17, ivanovMath.Sum);
        Assert.Equal(2, ivanovMath.Count);
        var ivanovHistory = Assert.Single(stats, s => s.StudentId == ivanov.Id && s.SubjectId == history.Id);
        Assert.Equal(7, ivanovHistory.Sum);
        var petrovMath = Assert.Single(stats, s => s.StudentId == petrov.Id);
        Assert.Equal(5, petrovMath.Sum);
        Assert.Equal(1, petrovMath.Count);
    }

    [Fact]
    public async Task GetStatsForClassAsync_NoGradesInPeriod_ReturnsEmpty()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);
        var subject = await SeedSubjectAsync("Математика");
        await SeedGradeAsync(student.Id, subject.Id, 8, new DateTime(2026, 8, 31));

        var stats = await _repository.GetStatsForClassAsync(schoolClass.Id, new DateTime(2026, 9, 1), new DateTime(2026, 10, 31));

        Assert.Empty(stats);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingGrade_ReturnsGrade()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);
        var subject = await SeedSubjectAsync("Математика");
        var grade = await SeedGradeAsync(student.Id, subject.Id, 6);

        var loaded = await _repository.GetByIdAsync(grade.Id);

        Assert.NotNull(loaded);
        Assert.Equal(6, loaded.Value);
    }

    [Fact]
    public async Task GetByIdAsync_MissingGrade_ReturnsNull()
    {
        var loaded = await _repository.GetByIdAsync(999);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task AddAsync_NewGrade_IsSaved()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);
        var subject = await SeedSubjectAsync("Математика");

        await _repository.AddAsync(new Grade { StudentId = student.Id, SubjectId = subject.Id, Value = 10, Date = new DateTime(2026, 9, 15) });

        await using var context = CreateContext();
        var saved = await context.Grades.SingleAsync();
        Assert.Equal(10, saved.Value);
        Assert.Equal(new DateTime(2026, 9, 15), saved.Date);
    }

    [Fact]
    public async Task UpdateAsync_ExistingGrade_ChangesValueAndDate()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);
        var subject = await SeedSubjectAsync("Математика");
        var grade = await SeedGradeAsync(student.Id, subject.Id, 5);

        var updated = await _repository.UpdateAsync(new Grade
        {
            Id = grade.Id,
            StudentId = student.Id,
            SubjectId = subject.Id,
            Value = 9,
            Date = new DateTime(2026, 10, 5)
        });

        Assert.True(updated);
        await using var context = CreateContext();
        var saved = await context.Grades.SingleAsync(g => g.Id == grade.Id);
        Assert.Equal(9, saved.Value);
        Assert.Equal(new DateTime(2026, 10, 5), saved.Date);
    }

    [Fact]
    public async Task UpdateAsync_MissingGrade_ReturnsFalse()
    {
        var updated = await _repository.UpdateAsync(new Grade { Id = 999, Value = 5 });

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteAsync_ExistingGrade_RemovesGrade()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);
        var subject = await SeedSubjectAsync("Математика");
        var grade = await SeedGradeAsync(student.Id, subject.Id);

        var deleted = await _repository.DeleteAsync(grade.Id);

        Assert.True(deleted);
        await using var context = CreateContext();
        Assert.False(await context.Grades.AnyAsync());
    }

    [Fact]
    public async Task DeleteAsync_MissingGrade_ReturnsFalse()
    {
        var deleted = await _repository.DeleteAsync(999);

        Assert.False(deleted);
    }
}
