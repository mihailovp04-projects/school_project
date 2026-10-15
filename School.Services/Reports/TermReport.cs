using School.Domain;

namespace School.Services.Reports;

public sealed class TermReport
{
    public required AcademicTerm Term { get; init; }

    public required SchoolClass SchoolClass { get; init; }

    public required IReadOnlyList<Subject> Subjects { get; init; }

    public required IReadOnlyList<TermReportRow> Rows { get; init; }

    public required IReadOnlyDictionary<int, double?> SubjectAverages { get; init; }

    public required bool HasGrades { get; init; }
}
