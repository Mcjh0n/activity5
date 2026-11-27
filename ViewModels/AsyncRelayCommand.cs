using System.Windows.Input;

namespace HubLink.Desktop.ViewModels;

public sealed class AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !IsExecuting && canExecute();
    private bool IsExecuting { get; set; }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        IsExecuting = true;
        NotifyCanExecuteChanged();
        try
        {
            await execute();
        }
        finally
        {
            IsExecuting = false;
            NotifyCanExecuteChanged();
        }
    }

    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
