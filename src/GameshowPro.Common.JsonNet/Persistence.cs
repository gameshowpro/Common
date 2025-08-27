using System.Threading.Tasks;

namespace GameshowPro.Common.JsonNet;

/// <summary>
/// An implementation of IPersistence using Json.NET
/// </summary>
public class Persistence : Model.IPersistence
{
    internal Persistence() { }

    /// <summary>
    /// Persists an object to a file path using Json.NET.
    /// </summary>
    /// <typeparam name="T">The type of the object to persist.</typeparam>
    /// <param name="obj">The object instance to persist.</param>
    /// <param name="path">Destination file path; if null, nothing is written.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="cancellationToken">Optional cancellation token (not currently observed).</param>
    /// <remarks>Docs added by AI.</remarks>
    public Task Persist<T>(T obj, string? path, ILogger? logger, CancellationToken? cancellationToken)
    {
        JsonNetUtils.Persist(obj, path);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Loads an object from a JSON file using Json.NET.
    /// </summary>
    /// <typeparam name="T">The target type to load.</typeparam>
    /// <param name="path">Source file path.</param>
    /// <param name="rethrowDeserializationExceptions">If true, rethrows deserialization exceptions; otherwise logs and returns default.</param>
    /// <param name="renameFailedFiles">If true, renames unreadable files before returning default.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="cancellationToken">Optional cancellation token (not currently observed).</param>
    /// <returns>The deserialized instance, or null when a new object had to be created.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public Task<T?> Depersist<T>(string? path, bool rethrowDeserializationExceptions, bool renameFailedFiles, ILogger? logger, CancellationToken? cancellationToken) where T : new()
    {
        T result = JsonNetUtils.Depersist<T>(path, null, out bool isNew, logger, rethrowDeserializationExceptions, renameFailedFiles);
        return Task.FromResult(isNew ? default : result);
    }
}
