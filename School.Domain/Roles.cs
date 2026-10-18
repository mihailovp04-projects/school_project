namespace School.Domain;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";

    public static bool IsKnown(string? role) => role is Admin or Teacher;
}
