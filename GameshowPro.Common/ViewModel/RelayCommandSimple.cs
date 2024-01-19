// (C) Barjonas LLC 2018

namespace GameshowPro.Common.ViewModel;

/// <summary>
/// An ICommand implementation with no parameters and no built-in CanExecute functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
/// </remarks>
/// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
/// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
public class RelayCommandSimple(Action execute) : ICommand
{
    #region Fields

    protected readonly Action _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    #endregion
    #region Constructors

    public void SetCanExecute(bool canExecute)
    {
        if (_canExecute != canExecute)
        {
            _canExecute = canExecute;
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }

    private bool _canExecute = true;

    #endregion

    #region ICommand Members

    ///<summary>
    ///Defines the method that determines whether the command can execute in its current state.
    ///</summary>
    ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    ///<returns>
    ///true if this command can be executed; otherwise, false.
    ///</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute;
    }

    ///<summary>
    ///Occurs when changes occur that affect whether or not the command should execute.
    ///</summary>
    public event EventHandler? CanExecuteChanged;

    ///<summary>
    ///Defines the method to be called when the command is invoked.
    ///</summary>
    ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
    public virtual void Execute(object? parameter)
    {
        _execute();
    }
    #endregion
}
