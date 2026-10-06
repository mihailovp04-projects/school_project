using Microsoft.EntityFrameworkCore;
using School.Domain;

namespace School.Data;

public class SchoolDbContext : DbContext
{
    public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<SchoolClass> SchoolClasses { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Grade> Grades { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SchoolClass>()
            .HasMany(c => c.Subjects)
            .WithMany(s => s.SchoolClasses);

        modelBuilder.Entity<Student>()
            .HasOne(s => s.SchoolClass)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.SchoolClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Subject)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SchoolClass>()
            .Property(c => c.Name)
            .HasMaxLength(10);

        modelBuilder.Entity<SchoolClass>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Subject>()
            .HasIndex(s => s.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Login)
            .HasMaxLength(256);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();
    }
}