using System.ComponentModel.DataAnnotations;

namespace School.Services.Validation;

public static class EntityValidator
{
    public static void Validate(object entity)
    {
        var context = new ValidationContext(entity);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(entity, context, results, validateAllProperties: true))
        {
            var message = string.Join(" ", results.Select(r => r.ErrorMessage));
            throw new ArgumentException(message);
        }
    }
}