using System.ComponentModel.DataAnnotations;

namespace School.Domain;

public class AcademicTerm : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название периода обязательно для заполнения.")]
    [StringLength(50, ErrorMessage = "Название периода не должно превышать 50 символов.")]
    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsClosed { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate == default)
        {
            yield return new ValidationResult(
                "Укажите дату начала периода.",
                new[] { nameof(StartDate) });
        }

        if (EndDate == default)
        {
            yield return new ValidationResult(
                "Укажите дату окончания периода.",
                new[] { nameof(EndDate) });
        }
        else if (StartDate != default && EndDate.Date <= StartDate.Date)
        {
            yield return new ValidationResult(
                "Дата окончания периода должна быть позже даты начала.",
                new[] { nameof(EndDate) });
        }
    }
}
