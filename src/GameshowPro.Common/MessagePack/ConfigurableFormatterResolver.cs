using MessagePack;
using MessagePack.Formatters;

namespace GameshowPro.Common.MessagePack;

/// <summary>
/// Resolver that allows runtime formatter registration and ordered resolver chaining.
/// </summary>
public sealed class ConfigurableFormatterResolver : IFormatterResolver
{
    private readonly ConcurrentDictionary<Type, object?> _cache = new();
    private readonly Dictionary<Type, object> _registeredFormatters = [];
    private readonly List<IFormatterResolver> _subResolvers = [];
    private readonly object _syncRoot = new();

    /// <summary>
    /// Registers a formatter for <typeparamref name="T"/> with highest resolution priority.
    /// </summary>
    public ConfigurableFormatterResolver RegisterFormatter<T>(IMessagePackFormatter<T> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        lock (_syncRoot)
        {
            _registeredFormatters[typeof(T)] = formatter;
            _cache.Clear();
        }

        return this;
    }

    /// <summary>
    /// Adds a child resolver that is queried in registration order.
    /// </summary>
    public ConfigurableFormatterResolver AddResolver(IFormatterResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        lock (_syncRoot)
        {
            _subResolvers.Add(resolver);
            _cache.Clear();
        }

        return this;
    }

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        return (IMessagePackFormatter<T>?)_cache.GetOrAdd(
            typeof(T),
            static (_, resolver) => resolver.ResolveFormatter<T>(),
            this);
    }

    private object? ResolveFormatter<T>()
    {
        lock (_syncRoot)
        {
            if (_registeredFormatters.TryGetValue(typeof(T), out object? formatter))
            {
                return formatter;
            }

            foreach (IFormatterResolver resolver in _subResolvers)
            {
                IMessagePackFormatter<T>? resolvedFormatter = resolver.GetFormatter<T>();
                if (resolvedFormatter is not null)
                {
                    return resolvedFormatter;
                }
            }

            return null;
        }
    }
}