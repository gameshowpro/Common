﻿namespace GameshowPro.Common.ViewModel;

/// <summary>
/// Defines a command in a cross-platform application. This is equivalent to the <see cref="Windows.Input.ICommand"/> interface that is used in Windows builds.
/// </summary>
public interface ICommand
{
    /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>    
    event EventHandler? CanExecuteChanged;

    /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>   
    bool CanExecute(object? parameter);
    /// <summary>Defines the method to be called when the command is invoked.</summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    void Execute(object? parameter);
}
