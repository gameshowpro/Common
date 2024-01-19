// (C) Barjonas LLC 2021


namespace GameshowPro.Common.ViewModel;

/// <summary>
/// An ICommand implementation with one parameter and customizable CanExecute functionality for executing an async function.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Creates a new command.
/// </remarks>
/// <param name="execute">The execution logic.</param>
/// <param name="canExecute">The execution status logic.</param>
public class AsyncCommand<T>(
    Func<T, Task> execute,
    Func<T, bool>? canExecute = null,
    Action<Exception>? errorHandler = null
    ) : IAsyncCommand<T> where T : notnull
{
    #region Fields
    private bool _isExecuting;
    private readonly Func<T, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<T, bool>? _canExecute = canExecute;
    private readonly Action<Exception>? _errorHandler = errorHandler;

    #endregion
    #region Constructors
    #endregion

    #region IAsyncCommand Members
    ///<summary>
    ///Defines the method that determines whether the command can execute in its current state.
    ///</summary>
    ///<param name="parameter">Data used by the command.  This data is ignored in this implementation.</param>
    ///<returns>
    ///true if this command can be executed; otherwise, false.
    ///</returns>
    public virtual bool CanExecute(T parameter)
        => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

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
    ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
    public async Task ExecuteAsync(T parameter)
    {
        if (CanExecute(parameter))
        {
            try
            {
                _isExecuting = true;
                await _execute(parameter);
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
        => parameter is null || CanExecute((T)parameter);

    void ICommand.Execute(object? parameter)
    {
        if (parameter != null)
        {
            ExecuteAsync((T)parameter).FireAndForgetSafeAsync(_errorHandler);
        }
    }
    #endregion
}

