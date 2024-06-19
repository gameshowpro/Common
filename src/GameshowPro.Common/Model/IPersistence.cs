namespace GameshowPro.Common.Model;

/// <summary>
/// Common interface between all supported serialization library wrappers.
/// </summary>
public interface IPersistence
{
    public void Persist<T>(T obj, string? path, bool enumsAsStrings = true);
    public T Depersist<T>(string? path, out bool isNew, ILogger? logger = null, bool rethrowDeserializationExceptions = false, bool renameFailedFiles = true) where T : new();
}
