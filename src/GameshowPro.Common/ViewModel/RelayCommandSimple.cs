// (C) Barjonas LLC 2024

namespace GameshowPro.Common.ViewModel;

/// <summary>
/// An ICommand implementation with no parameters and customizable CanExecute functionality.
/// </summary>
/// <param name="execute">Delegate to execute when Execute is called on the command.</param>
/// <param name="canExecute">The execution status logic, which has no parameters to act upon.</param>
public class RelayCommandSimple(Action execute, Func<bool>? canExecute) : ICommand
{
    #region Fields
    protected readonly Action _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    protected readonly Func<bool>? _canExecute = canExecute;

    #endregion
    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="RelayCommandSimple"/> without predefined <seealso cref="CanExecute"/> logic.
    /// </summary>
    /// <param name="execute">Delegate to execute when Execute is called on the command.</param>
    /// <remarks><seealso cref="CanExecute"/> will always return true unless changed using <see cref="SetCanExecute(bool)"/>.</remarks>
    public RelayCommandSimple(Action execute)
        : this(execute, null)
    {
    }
    #endregion

    #region CanExecute
    /// <summary>
    /// Set the CanExecute flag using external logic, raising <see cref="CanExecuteChanged"/> as appropriate.
    /// </summary>
    /// <param name="canExecute">The new value.</param>
    /// <exception cref="InvalidOperationException">Raised if <c>Func&lt;bool&gt;</c> was supplied in constructor.</exception>
    public void SetCanExecute(bool canExecute)
    {
        if (_canExecute != null)
        {
            throw new InvalidOperationException($"{nameof(SetCanExecute)} cannot be used in instances instantiated with a ${nameof(Func<bool>)}");
        }
        SetCanExecuteCommon(canExecute);
    }

    private bool _canExecuteBool = true;

    /// <summary>
    /// Run pre-defined CanExecute logic and raise <see cref="CanExecuteChanged"/> if this results in a change. Since this <see cref="ICommand"/> does not support parameters, this is only one possible result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Raised if <c>Func&lt;bool&gt;</c> was not supplied in constructor.</exception>
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

    #region ICommand Members
    ///<summary>
    ///Defines the method that determines whether the command can execute in its current state.
    ///</summary>
    ///<param name="parameter">Not supported in this implementation.</param>
    ///<returns>
    ///true if this command can be executed; otherwise, false.
    ///</returns>
    public virtual bool CanExecute(object? parameter)
        => _canExecuteBool;

    ///<summary>
    ///Occurs when changes occur that affect whether or not the command should execute.
    ///</summary>
    public event EventHandler? CanExecuteChanged;

    ///<summary>
    ///Defines the method to be called when the command is invoked.
    ///</summary>
    ///<param name="parameter">Not supported in this implementation.</param>
    public virtual void Execute(object? parameter)
    {
        _execute();
    }
    #endregion
}
