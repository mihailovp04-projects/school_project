using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using School.Domain.Exceptions;

namespace School.Data;

internal static class DbContextSaveExtensions
{
    private const int SqlServerUniqueIndexViolation = 2601;
    private const int SqlServerUniqueConstraintViolation = 2627;
    private const int SqlServerForeignKeyViolation = 547;

    public static async Task SaveChangesCheckingUniqueAsync(this DbContext context, string value)
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new DuplicateNameException($"Value '{value}' violates a unique index.", ex);
        }
    }

    public static async Task SaveChangesCheckingReferencesAsync(this DbContext context)
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            throw new EntityInUseException("The record is referenced by other records.", ex);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException switch
        {
            SqlException sqlException => sqlException.Number is SqlServerUniqueIndexViolation or SqlServerUniqueConstraintViolation,
            Exception inner => inner.Message.Contains("UNIQUE constraint failed", StringComparison.Ordinal),
            null => false
        };

    private static bool IsForeignKeyViolation(DbUpdateException exception) =>
        exception.InnerException switch
        {
            SqlException sqlException => sqlException.Number == SqlServerForeignKeyViolation,
            Exception inner => inner.Message.Contains("FOREIGN KEY constraint failed", StringComparison.Ordinal),
            null => false
        };
}