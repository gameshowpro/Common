// (C) Barjonas LLC 2021

namespace GameshowPro.Common.ViewModel;

/// <summary>
/// An ICommand implementation with one parameter and customizable CanExecute functionality for executing an async function.
/// </summary>
/// <remarks>
/// Creates a new command.
/// </remarks>
/// <param name="execute">The execution logic.</param>
/// <param name="canExecute">The execution status logic.</param>
public class AsyncCommandSimple(
    Func<Task> execute,
    Func<bool>? canExecute = null,
    Action<Exception>? errorHandler = null
    ) : IAsyncCommand<object?>
{
    #region Fields
    private bool _isExecuting;
    private readonly Func<Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<bool>? _canExecute = canExecute;
    private readonly Action<Exception>? _errorHandler = errorHandler;

    #endregion
    #region Constructors
    #endregion

    #region IAsyncCommand Members
    ///<summary>
    ///Defines the method that determines whether the command can execute in its current state.
    ///</summary>
    ///<param name="parameter">This parameter is ignored in this implementation.</param>
    ///<returns>
    ///true if this command can be executed; otherwise, false.
    ///</returns>
    public virtual bool CanExecute(object? parameter)
        => !_isExecuting && (_canExecute?.Invoke() ?? true);

    ///<summary>
    ///Defines the method that determines whether the command can execute in its current state.
    ///</summary>
    ///<returns>
    ///true if this command can be executed; otherwise, false.
    ///</returns>
    public bool CanExecute()
        => CanExecute(null);

    ///<summary>
    ///Occurs when changes occur that affect whether or not the command should execute.
    ///</summary>
    public event EventHandler? CanExecuteChanged;
    //{
    //    add { CommandManager.RequerySuggested += value; }
    //    remove { CommandManager.RequerySuggested -= value; }
    //}

    ///<summary>
    ///Defines the method to be called when the command is invoked.
    ///</summary>
    ///<param name="parameter">Data used by the command, which is ignored by this implementation.</param>
    public async Task ExecuteAsync(object? parameter)
    {
        if (CanExecute())
        {
            try
            {
                _isExecuting = true;
                await _execute();
            }
            finally
            {
                _isExecuting = false;
            }
        }
        RaiseCanExecuteChanged();
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region ICommand
    bool ICommand.CanExecute(object? parameter)
        => CanExecute(null);

    void ICommand.Execute(object? parameter)
        => ExecuteAsync(null).FireAndForgetSafeAsync(_errorHandler);
    #endregion
}
