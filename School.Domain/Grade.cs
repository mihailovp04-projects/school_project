using System.ComponentModel.DataAnnotations;

namespace School.Domain;

public class Grade : IValidatableObject
{
    public int Id { get; set; }

    [Range(1, 10, ErrorMessage = "Оценка должна быть от 1 до 10.")]
    public int Value { get; set; }

    public DateTime Date { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Date == default || Date > DateTime.Now)
        {
            yield return new ValidationResult(
                "Дата оценки не может быть в будущем.",
                new[] { nameof(Date) });
        }
    }
}