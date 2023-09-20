namespace Barjonas.Common.Model;

public interface IListChild<T> where T : IListChild<T>
{
    IList<T>? Parent { get; }
}
