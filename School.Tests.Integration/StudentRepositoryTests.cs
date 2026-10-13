using Microsoft.EntityFrameworkCore;
using School.Data.Contracts.Exceptions;
using School.Data.Repositories;
using School.Domain;

namespace School.Tests.Integration;

public class StudentRepositoryTests : SqliteTestBase
{
    private readonly StudentRepository _repository;

    public StudentRepositoryTests()
    {
        _repository = new StudentRepository(ContextFactory);
    }

    [Fact]
    public async Task DeleteAsync_StudentWithGrades_ThrowsEntityInUseException()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);
        var subject = await SeedSubjectAsync("Математика");
        await SeedGradeAsync(student.Id, subject.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _repository.DeleteAsync(student.Id));

        await using var context = CreateContext();
        Assert.True(await context.Students.AnyAsync(s => s.Id == student.Id));
        Assert.Equal(1, await context.Grades.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_StudentWithoutGrades_RemovesStudent()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var student = await SeedStudentAsync(schoolClass.Id);

        var deleted = await _repository.DeleteAsync(student.Id);

        Assert.True(deleted);
        await using var context = CreateContext();
        Assert.False(await context.Students.AnyAsync(s => s.Id == student.Id));
    }

    [Fact]
    public async Task DeleteAsync_MissingStudent_ReturnsFalse()
    {
        var deleted = await _repository.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task AddAsync_NewStudent_CanBeLoadedWithClassAndSubjects()
    {
        var schoolClass = await SeedClassAsync("5-А");
        var subject = await SeedSubjectAsync("Математика");
        await AssignSubjectAsync(schoolClass.Id, subject.Id);
        var student = new Student
        {
            FirstName = "Пётр",
            LastName = "Петров",
            BirthDate = new DateTime(2013, 5, 20),
            Phone = "+37369123456",
            SchoolClassId = schoolClass.Id
        };

        await _repository.AddAsync(student);
        var loaded = await _repository.GetByIdAsync(student.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Петров", loaded.LastName);
        Assert.Equal("5-А", loaded.SchoolClass.Name);
        Assert.Single(loaded.SchoolClass.Subjects);
    }

    [Fact]
    public async Task GetByIdAsync_MissingStudent_ReturnsNull()
    {
        var loaded = await _repository.GetByIdAsync(999);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsStudentsWithClass()
    {
        var schoolClass = await SeedClassAsync("5-А");
        await SeedStudentAsync(schoolClass.Id, "Иванов");
        await SeedStudentAsync(schoolClass.Id, "Петров");

        var students = await _repository.GetAllAsync();

        Assert.Equal(2, students.Count);
        Assert.All(students, s => Assert.Equal("5-А", s.SchoolClass.Name));
    }

    [Fact]
    public async Task UpdateAsync_ExistingStudent_ChangesFields()
    {
        var firstClass = await SeedClassAsync("5-А");
        var secondClass = await SeedClassAsync("6-Б");
        var student = await SeedStudentAsync(firstClass.Id);

        var updated = await _repository.UpdateAsync(new Student
        {
            Id = student.Id,
            FirstName = "Сергей",
            LastName = "Сидоров",
            BirthDate = new DateTime(2011, 1, 2),
            Phone = "+37360000000",
            SchoolClassId = secondClass.Id
        });

        Assert.True(updated);
        await using var context = CreateContext();
        var saved = await context.Students.SingleAsync(s => s.Id == student.Id);
        Assert.Equal("Сидоров", saved.LastName);
        Assert.Equal("Сергей", saved.FirstName);
        Assert.Equal("+37360000000", saved.Phone);
        Assert.Equal(secondClass.Id, saved.SchoolClassId);
    }

    [Fact]
    public async Task UpdateAsync_MissingStudent_ReturnsFalse()
    {
        var updated = await _repository.UpdateAsync(new Student { Id = 999, FirstName = "Иван", LastName = "Иванов", SchoolClassId = 1 });

        Assert.False(updated);
    }
}
