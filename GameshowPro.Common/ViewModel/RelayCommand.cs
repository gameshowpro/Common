// (C) Barjonas LLC 2024

namespace GameshowPro.Common.ViewModel;

/// <summary>
/// An ICommand implementation with one parameter and customizable CanExecute functionality.
/// </summary>
/// <typeparam name="T">The type of the parameter which this command supports</typeparam>
/// <param name="execute">The execution logic.</param>
/// <param name="canExecute">The execution status logic, which can be specific to particular parameters.</param>
public class RelayCommand<T>(Action<T?> execute, Predicate<T?>? canExecute) : ICommand
{
    #region Fields

    protected readonly Action<T?> _execute = execute;
    protected readonly Predicate<T?>? _canExecute = canExecute;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="RelayCommand{T}"/> without predefined <seealso cref="CanExecute"/> logic.
    /// </summary>
    /// <param name="execute">Delegate to execute when Execute is called on the command.</param>
    /// <remarks><seealso cref="CanExecute"/> will always return true unless changes using <see cref="SetCanExecute(bool)"/>.</remarks>
    public RelayCommand(Action<T?> execute)
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
            throw new InvalidOperationException($"{nameof(SetCanExecute)} cannot be used in instances instantiated with a ${nameof(Predicate<T?>)}");
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
            throw new InvalidOperationException($"{nameof(RequeryCanExecute)} can only be used in instances instantiated with a ${nameof(Predicate<T?>)}");
        }
        RaiseCanExecuteChanged();
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    #endregion

    #region ICommand Members

    ///<summary>
    ///Defines the method that determines whether the command can execute in its current state.
    ///</summary>
    ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    ///<returns>
    ///true if this command can be executed; otherwise, false.
    ///</returns>
    public virtual bool CanExecute(object? parameter)
    {
        if (_canExecute == null)
        {
            return _canExecuteBool;
        }
        if (parameter is T parameterT)
        {
            return _canExecute(parameterT);
        }
        return false;
    }

    ///<summary>
    ///Occurs when changes occur that affect whether or not the command should execute.
    ///</summary>
    public event EventHandler? CanExecuteChanged;
    
    ///<summary>
    ///Executes the delegate defined when this command was created.
    ///</summary>
    ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
    public virtual void Execute(object? parameter)
    {
        _execute?.Invoke((T?)parameter);
    }

    #endregion
}

