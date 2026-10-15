using Microsoft.EntityFrameworkCore;
using School.Data.Contracts.Exceptions;
using School.Data.Repositories;
using School.Domain;

namespace School.Tests.Integration;

public class SchoolClassRepositoryTests : SqliteTestBase
{
    private readonly SchoolClassRepository _repository;

    public SchoolClassRepositoryTests()
    {
        _repository = new SchoolClassRepository(ContextFactory);
    }

    [Fact]
    public async Task DeleteAsync_ClassWithStudents_ThrowsEntityInUseExceptionAndKeepsClass()
    {
        var schoolClass = await SeedClassAsync("5-А");
        await SeedStudentAsync(schoolClass.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _repository.DeleteAsync(schoolClass.Id));

        await using var context = CreateContext();
        Assert.True(await context.SchoolClasses.AnyAsync(c => c.Id == schoolClass.Id));
        Assert.Equal(1, await context.Students.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_EmptyClass_RemovesClass()
    {
        var schoolClass = await SeedClassAsync("5-А");

        var deleted = await _repository.DeleteAsync(schoolClass.Id);

        Assert.True(deleted);
        await using var context = CreateContext();
        Assert.False(await context.SchoolClasses.AnyAsync(c => c.Id == schoolClass.Id));
    }

    [Fact]
    public async Task DeleteAsync_MissingClass_ReturnsFalse()
    {
        var deleted = await _repository.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task AddAsync_DuplicateName_ThrowsDuplicateNameException()
    {
        await SeedClassAsync("5-А");

        await Assert.ThrowsAsync<DuplicateNameException>(() => _repository.AddAsync(new SchoolClass { Name = "5-А" }));
    }

    [Fact]
    public async Task AddAsync_NewClass_AssignsId()
    {
        var schoolClass = new SchoolClass { Name = "7-В" };

        await _repository.AddAsync(schoolClass);

        Assert.True(schoolClass.Id > 0);
    }

    [Fact]
    public async Task UpdateAsync_ExistingClass_ChangesName()
    {
        var schoolClass = await SeedClassAsync("5-А");

        var updated = await _repository.UpdateAsync(new SchoolClass { Id = schoolClass.Id, Name = "6-А" });

        Assert.True(updated);
        await using var context = CreateContext();
        var saved = await context.SchoolClasses.SingleAsync(c => c.Id == schoolClass.Id);
        Assert.Equal("6-А", saved.Name);
    }

    [Fact]
    public async Task UpdateAsync_NameOfAnotherClass_ThrowsDuplicateNameException()
    {
        await SeedClassAsync("5-А");
        var other = await SeedClassAsync("5-Б");

        await Assert.ThrowsAsync<DuplicateNameException>(() => _repository.UpdateAsync(new SchoolClass { Id = other.Id, Name = "5-А" }));
    }

    [Fact]
    public async Task UpdateAsync_MissingClass_ReturnsFalse()
    {
        var updated = await _repository.UpdateAsync(new SchoolClass { Id = 999, Name = "5-А" });

        Assert.False(updated);
    }

    [Fact]
    public async Task ExistsByNameAsync_ExcludeId_DoesNotCountRecordAsItsOwnDuplicate()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var other = await SeedClassAsync("5-Б");

        Assert.True(await _repository.ExistsByNameAsync("5-А"));
        Assert.False(await _repository.ExistsByNameAsync("5-А", schoolClass.Id));
        Assert.True(await _repository.ExistsByNameAsync("5-А", other.Id));
        Assert.False(await _repository.ExistsByNameAsync("9-В"));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingClass_ReturnsClassWithSubjects()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var subject = await SeedSubjectAsync("Математика");
        await AssignSubjectAsync(schoolClass.Id, subject.Id);

        var loaded = await _repository.GetByIdAsync(schoolClass.Id);

        Assert.NotNull(loaded);
        Assert.Equal("5-А", loaded.Name);
        Assert.Single(loaded.Subjects);
    }

    [Fact]
    public async Task GetByIdAsync_MissingClass_ReturnsNull()
    {
        var loaded = await _repository.GetByIdAsync(999);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsClassesWithStudentsAndSubjects()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var subject = await SeedSubjectAsync("Математика");
        await SeedStudentAsync(schoolClass.Id);
        await AssignSubjectAsync(schoolClass.Id, subject.Id);

        var classes = await _repository.GetAllAsync();

        var loaded = Assert.Single(classes);
        Assert.Single(loaded.Students);
        Assert.Single(loaded.Subjects);
    }

    [Fact]
    public async Task AddSubjectAsync_NotAssigned_AssignsSubject()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var subject = await SeedSubjectAsync("Математика");

        var assigned = await _repository.AddSubjectAsync(schoolClass.Id, subject.Id);

        Assert.True(assigned);
        Assert.Equal(1, await CountAssignedSubjectsAsync(schoolClass.Id));
    }

    [Fact]
    public async Task AddSubjectAsync_AlreadyAssigned_DoesNotDuplicate()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var subject = await SeedSubjectAsync("Математика");
        await AssignSubjectAsync(schoolClass.Id, subject.Id);

        var assigned = await _repository.AddSubjectAsync(schoolClass.Id, subject.Id);

        Assert.True(assigned);
        Assert.Equal(1, await CountAssignedSubjectsAsync(schoolClass.Id));
    }

    [Fact]
    public async Task AddSubjectAsync_MissingSubject_ReturnsFalse()
    {
        var schoolClass = await SeedClassAsync("5-А");

        var assigned = await _repository.AddSubjectAsync(schoolClass.Id, 999);

        Assert.False(assigned);
    }

    [Fact]
    public async Task RemoveSubjectAsync_Assigned_RemovesOnlyLink()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var subject = await SeedSubjectAsync("Математика");
        await AssignSubjectAsync(schoolClass.Id, subject.Id);

        var removed = await _repository.RemoveSubjectAsync(schoolClass.Id, subject.Id);

        Assert.True(removed);
        Assert.Equal(0, await CountAssignedSubjectsAsync(schoolClass.Id));
        await using var context = CreateContext();
        Assert.True(await context.Subjects.AnyAsync(s => s.Id == subject.Id));
    }

    [Fact]
    public async Task RemoveSubjectAsync_NotAssigned_ReturnsFalse()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var subject = await SeedSubjectAsync("Математика");

        var removed = await _repository.RemoveSubjectAsync(schoolClass.Id, subject.Id);

        Assert.False(removed);
    }

    [Fact]
    public async Task RemoveSubjectAsync_MissingClass_ReturnsFalse()
    {
        var removed = await _repository.RemoveSubjectAsync(999, 1);

        Assert.False(removed);
    }

    private async Task<int> CountAssignedSubjectsAsync(int schoolClassId)
    {
        await using var context = CreateContext();
        var schoolClass = await context.SchoolClasses
            .Include(c => c.Subjects)
            .SingleAsync(c => c.Id == schoolClassId);
        return schoolClass.Subjects.Count;
    }
}
