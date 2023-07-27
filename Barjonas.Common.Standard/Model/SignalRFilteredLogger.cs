
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Barjonas.Common.Model;

public sealed class SignalRFilteredLogger : ILogger
{
    private readonly Logger _logger;
    private readonly static Stopwatch s_stopwatch = Stopwatch.StartNew();
    private static readonly TimeSpan s_maximumInvocationTime = TimeSpan.FromSeconds(0.1);
    private readonly ConcurrentDictionary<string, TimeSpan> _invocationsInProgress = new();
    private readonly bool _allMessages;

    public SignalRFilteredLogger(Logger logger, bool allMessages)
    {
        _logger = logger;
        _allMessages = allMessages;
    }

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
        if (state is IReadOnlyList<KeyValuePair<string, object?>> stateTyped && stateTyped.Count > 0 && stateTyped[0].Key == "InvocationId" && stateTyped[0].Value is string invocationId)
        {
            string message = formatter(state, exception);
            _logger.Trace(message);
            if (eventId.Name == "InvocationCreated") //created
            {
                _invocationsInProgress.TryAdd(invocationId, s_stopwatch.Elapsed);
            }
            else if(eventId.Name == "InvocationDisposed")
            {
                if (_invocationsInProgress.TryRemove(invocationId, out TimeSpan creationTime))
                {
                    TimeSpan elapsed = s_stopwatch.Elapsed - creationTime;
                    _logger.Log(elapsed > s_maximumInvocationTime ? NLog.LogLevel.Warn : NLog.LogLevel.Trace,  "Invocation {id} lifetime was {elapsed}", invocationId, elapsed);
                }
                else
                {
                    _logger.Warn("Invocation timing failed. Disposal message received for {invocationId} without prior creation message.", invocationId);
                }
            }
        }
        else if (_allMessages)
        {
            string message = formatter(state, exception);
            _logger.Trace(() => formatter(state, exception));
        }
    }
}


/// <summary>
/// A custom ILoggerProvider which passes very specific log messages from SignalR to a given NLog logger. 
/// In particular, it's the only known way to log the invocation ID on the client end for comparison with the server without swamping the log with lots of other messages.
/// </summary>
[ProviderAlias(nameof(SignalRFilteredLogger))]
public sealed class SignalRFilteredProvider : ILoggerProvider
{
    private readonly Logger _logger;
    private readonly bool _allMessages;

    public SignalRFilteredProvider(Logger logger, bool allMessages)
    {
        _logger = logger;
        _allMessages = allMessages;
    }

    public ILogger CreateLogger(string categoryName) =>
       new SignalRFilteredLogger(_logger, _allMessages);


    public void Dispose() { }
}

