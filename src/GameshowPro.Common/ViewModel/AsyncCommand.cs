// (C) Barjonas LLC 2024

namespace GameshowPro.Common.ViewModel;

/// <summary>
/// An ICommand implementation for executing an async function with one parameter and customizable CanExecute functionality.
/// </summary>
/// <typeparam name="T">The type of the parameter which this command supports</typeparam>
/// <param name="execute">The execution logic.</param>
/// <param name="canExecute">The execution status logic, which can be specific to particular parameters.</param>
/// <param name="errorHandler">A delegate to handle any exception which my be raised during execution.</param>
public class AsyncCommand<T>(
    Func<T?, Task> execute,
    Func<T?, bool>? canExecute = null,
    Action<Exception>? errorHandler = null
    ) : IAsyncCommand<T> where T : notnull
{
    #region Fields
    private bool _isExecuting;
    private readonly Func<T?, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<T?, bool>? _canExecute = canExecute;
    private readonly Action<Exception>? _errorHandler = errorHandler;

    #endregion
    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="AsyncCommand{T}"/> without predefined <seealso cref="CanExecute"/> logic or an <see cref="Action{Exception}"/> to handle errors.
    /// </summary>
    /// <param name="execute">Delegate to execute when Execute is called on the command.</param>
    /// <remarks><seealso cref="CanExecute"/> will always return true unless changes using <see cref="SetCanExecute(bool)"/>.</remarks>
    public AsyncCommand(Func<T?, Task> execute)
        : this(execute, null)
    {
    }
    #endregion

    #region CanExecute
    /// <summary>
    /// Set the CanExecute flag using external logic, raising <see cref="CanExecuteChanged"/> as appropriate.
    /// </summary>
    /// <param name="canExecute">The new value.</param>
    /// <exception cref="InvalidOperationException">Raised if <see cref="Predicate'1"/> was supplied in constructor.</exception>
    public void SetCanExecute(bool canExecute)
    {
        if (_canExecute != null)
        {
            throw new InvalidOperationException($"{nameof(SetCanExecute)} cannot be used in instances instantiated with a ${nameof(Predicate<>)}");
        }
        if (_canExecuteBool != canExecute)
        {
            _canExecuteBool = canExecute;
            RaiseCanExecuteChanged();
        }
    }

    private bool _canExecuteBool = true;

    /// <summary>
    /// Raise <see cref="CanExecuteChanged"/> so that consumers can rerun <see cref="CanExecute"/> with their own parameters.
    /// </summary>
    /// <exception cref="InvalidOperationException">Raised if <see cref="Predicate'1"/> was not supplied in constructor.</exception>
    public void RequeryCanExecute()
    {
        if (_canExecute == null)
        {
            throw new InvalidOperationException($"{nameof(RequeryCanExecute)} can only be used in instances instantiated with a ${nameof(Predicate<>)}");
        }
        RaiseCanExecuteChanged();
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region IAsyncCommand Members
    ///<summary>
    ///Defines the method that determines whether the command can execute in its current state.
    ///</summary>
    ///<param name="parameter">Data used by the command.  This data is ignored in this implementation.</param>
    ///<returns>
    ///true if this command can be executed; otherwise, false.
    ///</returns>
    public virtual bool CanExecute(T? parameter)
        => !_isExecuting && _canExecuteBool;

    ///<summary>
    ///Occurs when changes occur that affect whether or not the command should execute.
    ///</summary>
    public event EventHandler? CanExecuteChanged;

    ///<summary>
    ///Defines the method to be called when the command is invoked.
    ///</summary>
    ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
    public async Task ExecuteAsync(T? parameter)
    {
        if (CanExecute(parameter))
        {
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _execute(parameter);
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
        => CanExecute(parameter == null ? default : (T?)parameter);

    void ICommand.Execute(object? parameter)
        => ExecuteAsync(parameter == null ? default : (T?)parameter).FireAndForgetSafeAsync(_errorHandler);
    #endregion
}

