// (C) Barjonas LLC 2024

namespace GameshowPro.Common.ViewModel;

/// <summary>
/// An ICommand implementation for executing an async function with no parameters and customizable CanExecute functionality.
/// </summary>
/// <param name="execute">Delegate to execute when Execute is called on the command.</param>
/// <param name="canExecute">The execution status logic, which has no parameters to act upon.</param>
/// <param name="errorHandler">A delegate to handle any exception which my be raised during execution.</param>
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
    /// <summary>
    /// Initializes a new instance of <see cref="AsyncCommandSimple"/> without predefined <seealso cref="CanExecute"/> logic or an <see cref="Action{Exception}"/> to handle errors.
    /// </summary>
    /// <param name="execute">Delegate to execute when Execute is called on the command.</param>
    /// <remarks><seealso cref="CanExecute"/> will always return true unless changed using <see cref="SetCanExecute(bool)"/>.</remarks>
    public AsyncCommandSimple(Func<Task> execute)
        : this(execute, null, null)
    {
    }
    #endregion

    #region CanExecute
    /// <summary>
    /// Set the CanExecute flag using external logic, raising <see cref="CanExecuteChanged"/> as appropriate.
    /// </summary>
    /// <param name="canExecute">The new value.</param>
    /// <exception cref="InvalidOperationException">Raised if <see cref="Func{bool}"/> was supplied in constructor.</exception>
    public void SetCanExecute(bool canExecute)
    {
        if (_canExecute != null)
        {
            throw new InvalidOperationException($"{nameof(SetCanExecute)} cannot be used in instances instantiated with a ${nameof(Func<bool>)}");
        }
        SetCanExecuteCommon(canExecute);
    }

    private bool _canExecuteBool = true;

    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Run pre-defined CanExecute logic and raise <see cref="CanExecuteChanged"/> if this results in a change. Since this <see cref="ICommand"/> does not support parameters, this is only one possible result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Raised if <see cref="Func{bool}"/> was not supplied in constructor.</exception>
    public void RequeryCanExecute()
    {
        if (_canExecute == null)
        {
            throw new InvalidOperationException($"{nameof(RequeryCanExecute)} can only be used in instances instantiated with a ${nameof(Func<bool>)}");
        }
        SetCanExecuteCommon(_canExecute.Invoke());
    }

    private void SetCanExecuteCommon(bool newValue)
    {
        if (_canExecuteBool != newValue)
        {
            _canExecuteBool = newValue;
            RaiseCanExecuteChanged();
        }
    }
    public void RaiseCanExecuteChanged()
    => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
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
        => !_isExecuting && _canExecuteBool;

    ///<summary>
    ///Defines the method that determines whether the command can execute in its current state.
    ///</summary>
    ///<returns>
    ///true if this command can be executed; otherwise, false.
    ///</returns>
    public bool CanExecute()
        => CanExecute(null);

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
                RaiseCanExecuteChanged();
                await _execute();
            }
            catch (Exception ex)
            {
                _errorHandler?.Invoke(ex);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }

    #endregion

    #region ICommand
    bool ICommand.CanExecute(object? parameter)
        => CanExecute(null);

    void ICommand.Execute(object? parameter)
        => ExecuteAsync(null).FireAndForgetSafeAsync(_errorHandler);
    #endregion
}
