// (C) Barjonas LLC 2021

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Barjonas.Common.ViewModel
{
    public interface IAsyncCommand<T> : ICommand
    {
        Task ExecuteAsync(T parameter);
        bool CanExecute(T parameter);
    }
}
