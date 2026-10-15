namespace School.Data.Contracts.Models;

public sealed class GradeStat
{
    public GradeStat(int studentId, int subjectId, int sum, int count)
    {
        StudentId = studentId;
        SubjectId = subjectId;
        Sum = sum;
        Count = count;
    }

    public int StudentId { get; }

    public int SubjectId { get; }

    public int Sum { get; }

    public int Count { get; }
}
