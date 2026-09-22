namespace School.Domain;

public class Grade
{
    public int Id { get; set; }
    public int Value { get; set; }
    public DateTime Date { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
}