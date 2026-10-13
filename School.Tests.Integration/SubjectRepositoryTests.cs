using Microsoft.EntityFrameworkCore;
using School.Data.Contracts.Exceptions;
using School.Data.Repositories;
using School.Domain;

namespace School.Tests.Integration;

public class SubjectRepositoryTests : SqliteTestBase
{
    private readonly SubjectRepository _repository;

    public SubjectRepositoryTests()
    {
        _repository = new SubjectRepository(ContextFactory);
    }

    [Fact]
    public async Task AddAsync_DuplicateName_ThrowsDuplicateNameException()
    {
        await SeedSubjectAsync("Математика");

        await Assert.ThrowsAsync<DuplicateNameException>(() => _repository.AddAsync(new Subject { Name = "Математика" }));
    }

    [Fact]
    public async Task UpdateAsync_ExistingSubject_ChangesName()
    {
        var subject = await SeedSubjectAsync("Математика");

        var updated = await _repository.UpdateAsync(new Subject { Id = subject.Id, Name = "Алгебра" });

        Assert.True(updated);
        await using var context = CreateContext();
        var saved = await context.Subjects.SingleAsync(s => s.Id == subject.Id);
        Assert.Equal("Алгебра", saved.Name);
    }

    [Fact]
    public async Task UpdateAsync_NameOfAnotherSubject_ThrowsDuplicateNameException()
    {
        await SeedSubjectAsync("Математика");
        var other = await SeedSubjectAsync("История");

        await Assert.ThrowsAsync<DuplicateNameException>(() => _repository.UpdateAsync(new Subject { Id = other.Id, Name = "Математика" }));
    }

    [Fact]
    public async Task UpdateAsync_MissingSubject_ReturnsFalse()
    {
        var updated = await _repository.UpdateAsync(new Subject { Id = 999, Name = "Химия" });

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteAsync_SubjectWithGrades_ThrowsEntityInUseException()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);
        var subject = await SeedSubjectAsync("Математика");
        await SeedGradeAsync(student.Id, subject.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _repository.DeleteAsync(subject.Id));

        await using var context = CreateContext();
        Assert.True(await context.Subjects.AnyAsync(s => s.Id == subject.Id));
    }

    [Fact]
    public async Task DeleteAsync_SubjectAssignedToClass_RemovesSubjectAndLink()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var subject = await SeedSubjectAsync("Математика");
        await AssignSubjectAsync(schoolClass.Id, subject.Id);

        var deleted = await _repository.DeleteAsync(subject.Id);

        Assert.True(deleted);
        await using var context = CreateContext();
        var loaded = await context.SchoolClasses.Include(c => c.Subjects).SingleAsync(c => c.Id == schoolClass.Id);
        Assert.Empty(loaded.Subjects);
    }

    [Fact]
    public async Task DeleteAsync_MissingSubject_ReturnsFalse()
    {
        var deleted = await _repository.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task ExistsByNameAsync_ExcludeId_DoesNotCountRecordAsItsOwnDuplicate()
    {
        var subject = await SeedSubjectAsync("Математика");

        Assert.True(await _repository.ExistsByNameAsync("Математика"));
        Assert.False(await _repository.ExistsByNameAsync("Математика", subject.Id));
    }

    [Fact]
    public async Task ExistsByNameAsync_SameNameInAnotherCase_IsNotCheckedHere()
    {
        await SeedSubjectAsync("Математика");

        // SQLite по умолчанию сравнивает строки с учётом регистра, а SQL Server в проде — без (коллация *_CI_*).
        // Поэтому «Математика» и «математика» здесь разные строки, хотя в проде это дубликат.
        // Этот тест фиксирует границу проверки на SQLite, а не поведение прода.
        Assert.False(await _repository.ExistsByNameAsync("математика"));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSubjectsWithClasses()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var subject = await SeedSubjectAsync("Математика");
        await AssignSubjectAsync(schoolClass.Id, subject.Id);

        var subjects = await _repository.GetAllAsync();

        var loaded = Assert.Single(subjects);
        Assert.Single(loaded.SchoolClasses);
    }
}
