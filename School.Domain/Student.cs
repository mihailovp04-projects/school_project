using System.ComponentModel.DataAnnotations;

namespace School.Domain;

public class Student : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Имя обязательно для заполнения.")]
    [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Фамилия обязательна для заполнения.")]
    [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов.")]
    public string LastName { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    [RegularExpression(@"^\+373\d{8}$", ErrorMessage = "Телефон должен быть в формате +373XXXXXXXX.")]
    public string? Phone { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Нужно выбрать класс.")]
    public int SchoolClassId { get; set; }

    public SchoolClass SchoolClass { get; set; } = null!;

    public ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (BirthDate == default || BirthDate > DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "Укажите корректную дату рождения (не в будущем).",
                new[] { nameof(BirthDate) });
        }
        else if (BirthDate < DateTime.UtcNow.AddYears(-100))
        {
            yield return new ValidationResult(
                "Дата рождения выглядит некорректной.",
                new[] { nameof(BirthDate) });
        }
    }
}