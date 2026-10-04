using System.ComponentModel.DataAnnotations;

namespace School.Domain;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Логин обязателен для заполнения.")]
    [StringLength(256, ErrorMessage = "Логин не должен превышать 256 символов.")]
    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    [Required(ErrorMessage = "Роль обязательна для заполнения.")]
    [RegularExpression("^(Admin|Teacher)$", ErrorMessage = "Роль должна быть либо 'Admin', либо 'Teacher'.")]
    public string Role { get; set; } = string.Empty;
}