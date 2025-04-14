using System.Threading.Tasks;

namespace GameshowPro.Common.JsonNet;

/// <summary>
/// An implementation of IPersistence using Json.NET
/// </summary>
public class Persistence : Model.IPersistence
{
    internal Persistence() { }

    public Task Persist<T>(T obj, string? path, ILogger? logger, CancellationToken? cancellationToken)
    {
        JsonNetUtils.Persist(obj, path);
        return Task.CompletedTask;
    }

    public Task<T?> Depersist<T>(string? path, bool rethrowDeserializationExceptions, bool renameFailedFiles, ILogger? logger, CancellationToken? cancellationToken) where T : new()
    {
        T result = JsonNetUtils.Depersist<T>(path, null, out bool isNew, logger, rethrowDeserializationExceptions, renameFailedFiles);
        return Task.FromResult(isNew ? default : result);
    }
}
