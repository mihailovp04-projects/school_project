namespace School.Data.Contracts.Exceptions;

public class EntityInUseException : Exception
{
    public EntityInUseException(string message) : base(message)
    {
    }

    public EntityInUseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
