// (C) Barjonas LLC 2018

using System;
using System.Windows.Input;
#nullable enable
namespace Barjonas.Common.ViewModel
{
    /// <summary>
    /// An ICommand implementation with one parameter and customizable CanExecute functionality.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        protected readonly Action<T?> _execute;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public RelayCommand(Action<T?> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic (currently ignored).</param>
        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute)
        {
            _execute = execute;
        }

        public void SetCanExecute(bool canExecute)
        {
            if (_canExecuteBool != canExecute)
            {
                _canExecuteBool = canExecute;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }

        private bool _canExecuteBool = true;

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
            return _canExecuteBool;
        }

        ///<summary>
        ///Occurs when changes occur that affect whether or not the command should execute.
        ///</summary>
        public event EventHandler? CanExecuteChanged;
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

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
}
#nullable restore
