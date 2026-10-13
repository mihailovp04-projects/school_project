using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using School.Data;
using School.Domain;

namespace School.Tests.Integration;

public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SchoolDbContext> _options;

    protected SqliteTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        _connection.Open();

        _options = new DbContextOptionsBuilder<SchoolDbContext>()
            .UseSqlite(_connection)
            .Options;

        ContextFactory = new TestDbContextFactory(_options);

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    protected IDbContextFactory<SchoolDbContext> ContextFactory { get; }

    protected SchoolDbContext CreateContext() => new(_options);

    protected async Task<SchoolClass> SeedClassAsync(string name)
    {
        await using var context = CreateContext();
        var schoolClass = new SchoolClass { Name = name };
        context.SchoolClasses.Add(schoolClass);
        await context.SaveChangesAsync();
        return schoolClass;
    }

    protected async Task<Subject> SeedSubjectAsync(string name)
    {
        await using var context = CreateContext();
        var subject = new Subject { Name = name };
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();
        return subject;
    }

    protected async Task<Student> SeedStudentAsync(int schoolClassId, string lastName = "Иванов")
    {
        await using var context = CreateContext();
        var student = new Student
        {
            FirstName = "Иван",
            LastName = lastName,
            BirthDate = new DateTime(2012, 3, 15),
            SchoolClassId = schoolClassId
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();
        return student;
    }

    protected async Task<Grade> SeedGradeAsync(int studentId, int subjectId, int value = 8)
    {
        await using var context = CreateContext();
        var grade = new Grade
        {
            StudentId = studentId,
            SubjectId = subjectId,
            Value = value,
            Date = new DateTime(2026, 10, 1)
        };
        context.Grades.Add(grade);
        await context.SaveChangesAsync();
        return grade;
    }

    protected async Task AssignSubjectAsync(int schoolClassId, int subjectId)
    {
        await using var context = CreateContext();
        var schoolClass = await context.SchoolClasses.Include(c => c.Subjects).SingleAsync(c => c.Id == schoolClassId);
        var subject = await context.Subjects.SingleAsync(s => s.Id == subjectId);
        schoolClass.Subjects.Add(subject);
        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed class TestDbContextFactory : IDbContextFactory<SchoolDbContext>
    {
        private readonly DbContextOptions<SchoolDbContext> _options;

        public TestDbContextFactory(DbContextOptions<SchoolDbContext> options)
        {
            _options = options;
        }

        public SchoolDbContext CreateDbContext() => new(_options);
    }
}
