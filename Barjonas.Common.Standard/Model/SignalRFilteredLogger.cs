#nullable enable
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Barjonas.Common.Model;

public sealed class SignalRFilteredLogger : ILogger
{
    private readonly Logger _logger;

    public SignalRFilteredLogger(Logger logger) =>
        _logger = logger;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) =>
        logLevel == Microsoft.Extensions.Logging.LogLevel.Trace;

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        if (state is IReadOnlyList<KeyValuePair<string, object?>> stateTyped && stateTyped.FirstOrDefault().Key == "InvocationId")
        {
            string message = formatter(state, exception);
            _logger.Trace(message);
        }

    }
}


/// <summary>
/// A custom ILoggerProvider which passes very specific log messages from SignalR to a given NLog logger. 
/// In particular, it's the only known way to log the invocation ID on the client end for comparison with the server without swaping the log with lots of other messages.
/// </summary>
[ProviderAlias(nameof(SignalRFilteredLogger))]
public sealed class SignalRFilteredProvider : ILoggerProvider
{
    private readonly Logger _logger;

    public SignalRFilteredProvider(Logger logger)
        => _logger = logger;

    public ILogger CreateLogger(string categoryName) =>
       new SignalRFilteredLogger(_logger);


    public void Dispose() { }
}
#nullable restore
