using School.Domain;

namespace School.Data.Contracts.Models;

public sealed class AcademicTermSummary
{
    public AcademicTermSummary(AcademicTerm term, int gradeCount)
    {
        Term = term;
        GradeCount = gradeCount;
    }

    public AcademicTerm Term { get; }

    public int GradeCount { get; }
}
