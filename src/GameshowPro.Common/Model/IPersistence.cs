namespace GameshowPro.Common.Model;

/// <summary>
/// Common interface between all supported serialization library wrappers.
/// </summary>
public interface IPersistence
{
    /// <summary>
    /// Persists an object to storage.
    /// </summary>
    /// <typeparam name="T">The type of the object to persist.</typeparam>
    /// <param name="obj">The object instance to persist.</param>
    /// <param name="path">Destination file path; if null, nothing is written.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <remarks>Docs added by AI.</remarks>
    public Task Persist<T>(T obj, string? path, ILogger? logger, CancellationToken? cancellationToken);

    /// <summary>
    /// Loads an object from storage.
    /// </summary>
    /// <typeparam name="T">The target type to load.</typeparam>
    /// <param name="path">Source file path.</param>
    /// <param name="rethrowDeserializationExceptions">If true, rethrows deserialization exceptions; otherwise logs and returns default.</param>
    /// <param name="renameFailedFiles">If true, renames unreadable files before returning default.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The deserialized instance, or null when deserialization failed or a new object would have been created.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public Task<T?> Depersist<T>(string? path, bool rethrowDeserializationExceptions, bool renameFailedFiles, ILogger? logger, CancellationToken? cancellationToken) where T : new();
}
