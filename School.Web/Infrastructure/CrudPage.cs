namespace School.Web.Infrastructure;

public abstract class CrudPage<TItem> : PageBase where TItem : class, new()
{
    protected TItem FormModel { get; set; } = new();

    protected int? EditingId { get; private set; }

    protected bool IsFormVisible { get; private set; }

    protected string? FormError { get; private set; }

    protected void OpenForm(TItem model, int? editingId)
    {
        FormModel = model;
        EditingId = editingId;
        IsFormVisible = true;
        FormError = null;
        PageMessage = null;
    }

    protected void CloseForm()
    {
        IsFormVisible = false;
        FormError = null;
    }

    protected async Task SaveFormAsync(string operation, Func<Task<bool>> save, Func<Task> reload)
    {
        var outcome = await RunAsync(operation, EditingId, save);
        if (!outcome.IsCompleted)
        {
            FormError = outcome.Message;
            return;
        }

        CloseForm();
        PageMessage = outcome.Message;
        await ReloadAsync(reload);
    }
}
