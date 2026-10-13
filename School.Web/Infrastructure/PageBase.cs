using Microsoft.AspNetCore.Components;

namespace School.Web.Infrastructure;

public abstract class PageBase : ComponentBase
{
    private ILogger? _logger;

    [Inject]
    protected ILoggerFactory LoggerFactory { get; set; } = default!;

    [Inject]
    protected AdminGuard AdminGuard { get; set; } = default!;

    protected ILogger Logger => _logger ??= LoggerFactory.CreateLogger(GetType());

    protected string? PageMessage { get; set; }

    protected async Task<ActionOutcome> RunAsync(string operation, int? id, Func<Task<bool>> action)
    {
        try
        {
            var completed = await action();
            return completed ? ActionOutcome.Done : ActionOutcome.NotFound;
        }
        catch (ArgumentException ex)
        {
            Logger.LogInformation("{Operation} rejected for Id {Id}: {Reason}", operation, id, ex.Message);
            return ActionOutcome.Rejected(ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "{Operation} failed for Id {Id}", operation, id);
            return ActionOutcome.Rejected(UiMessages.Unexpected);
        }
    }

    protected async Task RunPageActionAsync(string operation, int? id, Func<Task<bool>> action, Func<Task> reload)
    {
        PageMessage = null;
        var outcome = await RunAsync(operation, id, action);
        PageMessage = outcome.Message;

        if (outcome.IsCompleted)
        {
            await ReloadAsync(reload);
        }
    }

    protected async Task RunAdminActionAsync(string question, string operation, int? id, Func<Task<bool>> action, Func<Task> reload)
    {
        PageMessage = null;

        var confirmed = await AdminGuard.ConfirmAdminActionAsync(question);
        if (confirmed == null)
        {
            PageMessage = UiMessages.NoPermission;
            return;
        }

        if (confirmed == true)
        {
            await RunPageActionAsync(operation, id, action, reload);
        }
    }

    protected async Task ReloadAsync(Func<Task> reload)
    {
        var outcome = await RunAsync("Reload", null, async () =>
        {
            await reload();
            return true;
        });

        if (outcome.Message != null)
        {
            PageMessage = outcome.Message;
        }
    }
}
