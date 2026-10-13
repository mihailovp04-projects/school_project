namespace School.Web.Infrastructure;

public sealed class ActionOutcome
{
    private ActionOutcome(bool isCompleted, string? message)
    {
        IsCompleted = isCompleted;
        Message = message;
    }

    public bool IsCompleted { get; }

    public string? Message { get; }

    public static ActionOutcome Done { get; } = new(true, null);

    public static ActionOutcome NotFound { get; } = new(true, UiMessages.NotFound);

    public static ActionOutcome Rejected(string message) => new(false, message);
}
