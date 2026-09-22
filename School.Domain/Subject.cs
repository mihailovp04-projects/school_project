namespace School.Domain;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; 

    public ICollection<SchoolClass> SchoolClasses { get; set; } = new List<SchoolClass>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}