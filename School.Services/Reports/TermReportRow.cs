namespace School.Services.Reports;

public sealed class TermReportRow
{
    public required string StudentName { get; init; }

    public required IReadOnlyDictionary<int, double?> SubjectAverages { get; init; }

    public required double? Average { get; init; }

    public required int? FinalGrade { get; init; }
}
