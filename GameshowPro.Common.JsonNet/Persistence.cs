namespace GameshowPro.Common.JsonNet;

/// <summary>
/// An implementation of IPersistence using Json.NET
/// </summary>
public class Persistence : Model.IPersistence
{
    public T Depersist<T>(string? path, out bool isNew, ILogger? logger = null, bool rethrowDeserializationExceptions = false, bool renameFailedFiles = true) where T : new()
        => JsonNetUtils.Depersist<T>(path, null, out isNew, logger, rethrowDeserializationExceptions, renameFailedFiles);

    public void Persist<T>(T obj, string? path, bool enumsAsStrings = true)
        => JsonNetUtils.Persist(obj, path, enumsAsStrings);
}
