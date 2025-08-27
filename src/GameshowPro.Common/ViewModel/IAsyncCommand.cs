// (C) Barjonas LLC 2024

namespace GameshowPro.Common.ViewModel;

/// <summary>
/// An asynchronous command contract with a typed parameter, extending ICommand.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
/// <typeparam name="T">The parameter type.</typeparam>
public interface IAsyncCommand<T> : ICommand
{
    /// <summary>Execute the command asynchronously.</summary>
    /// <param name="parameter">The parameter value.</param>
    /// <remarks>Docs added by AI.</remarks>
    Task ExecuteAsync(T? parameter);
    /// <summary>Determine whether the command can execute with the provided parameter.</summary>
    /// <param name="parameter">The parameter value.</param>
    /// <remarks>Docs added by AI.</remarks>
    bool CanExecute(T? parameter);
}
