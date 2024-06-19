// (C) Barjonas LLC 2024

namespace GameshowPro.Common.ViewModel;

public interface IAsyncCommand<T> : ICommand
{
    Task ExecuteAsync(T? parameter);
    bool CanExecute(T? parameter);
}
