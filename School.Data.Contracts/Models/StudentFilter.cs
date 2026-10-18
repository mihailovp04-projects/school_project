namespace School.Data.Contracts.Models;

public sealed class StudentFilter
{
    public int? SchoolClassId { get; init; }

    public string? LastName { get; init; }
}
