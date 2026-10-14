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
