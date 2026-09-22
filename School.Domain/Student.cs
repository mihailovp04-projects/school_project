namespace School.Domain;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? Phone { get; set; }

    public int SchoolClassId { get; set; }
    public SchoolClass SchoolClass { get; set; } = null!;

    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}