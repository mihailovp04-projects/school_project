namespace School.Domain.Exceptions;

public class DuplicateNameException : Exception
{
    public DuplicateNameException(string message) : base(message)
    {
    }

    public DuplicateNameException(string message, Exception innerException) : base(message, innerException)
    {
    }
}