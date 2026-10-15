namespace School.Services.Reports;

public interface ITermReportService
{
    Task<TermReport?> BuildAsync(int termId, int schoolClassId);
}
