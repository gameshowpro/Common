namespace GameshowPro.Common.Model;

/// <summary>
/// Common interface between all supported serialization library wrappers.
/// </summary>
public interface IPersistence
{
    public Task Persist<T>(T obj, string? path, ILogger? logger, CancellationToken? cancellationToken);
    public Task<T?> Depersist<T>(string? path, bool rethrowDeserializationExceptions, bool renameFailedFiles, ILogger? logger, CancellationToken? cancellationToken) where T : new();
}
