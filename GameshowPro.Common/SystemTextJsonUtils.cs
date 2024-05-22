using System.Text.Json;
namespace GameshowPro.Common;

public static class SystemTextJsonUtils
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = true
    };

    static SystemTextJsonUtils()
    {
        s_jsonSerializerOptions.Converters.Add(new JsonConverters.TypeConverter());
        s_jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public static async Task<T> Depersist<T>(string? path, ILogger? logger = null, bool rethrowDeserializationExceptions = false, bool renameFailedFiles = true) where T : new()
    {
        T? obj = default;
        if (path is not null && File.Exists(path))
        {
            bool renameBroken = false;
            using FileStream fileStream = File.OpenRead(path);
            try
            {
                obj = await JsonSerializer.DeserializeAsync<T>(fileStream, s_jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Exception while deserializing {path}", path);
                if (renameFailedFiles)
                {
                    renameBroken = true;
                }
                if (rethrowDeserializationExceptions)
                {
                    throw new JsonException($"Exception while deserializing {path}", ex);
                }
            }
            
            if (renameBroken)
            {
                RenameBrokenFile(path, logger);
            }
        }
        if (obj == null)
        {
            obj = new T();
            logger?.LogInformation("Created new object because nothing could be deserialized from {path}", path);
        }
        else
        {
            logger?.LogInformation("Successfully deserialized from {path}", path);
        }
        return obj;
    }

    /// <summary>
    /// Persist any type to a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of object to be persisted.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="obj">Object to be persisted.</param>
    public static async Task Persist<T>(T obj, string? path, ILogger? logger, CancellationToken? cancellationToken)
    {
        if (obj is null || path is null)
        {
            return;
        }
        if (!EnsureDirectory(path))
        {
            logger?.LogError("Failed to find or create directory for {path}", path);
            return;
        }
        await using FileStream fileStream = File.Create(path);
        await JsonSerializer.SerializeAsync(fileStream, obj, s_jsonSerializerOptions, cancellationToken ?? default);
    }
}
