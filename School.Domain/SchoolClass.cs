using System.ComponentModel.DataAnnotations;

namespace School.Domain;

public class SchoolClass
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название класса обязательно для заполнения.")]
    [RegularExpression(@"^\d{1,2}-[А-Яа-яA-Za-z]$", ErrorMessage = "Название класса должно быть в формате 9-А или 11-Б.")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}