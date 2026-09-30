using System.ComponentModel.DataAnnotations;

namespace School.Domain;

public class Subject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название предмета обязательно для заполнения.")]
    [StringLength(50, ErrorMessage = "Название предмета не должно превышать 50 символов.")]
    public string Name { get; set; } = string.Empty;

    public ICollection<SchoolClass> SchoolClasses { get; set; } = new List<SchoolClass>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}