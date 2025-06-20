namespace GameshowPro.Common;

public static class JsonNetUtils
{
    /// <summary>
    /// Load any Type from a JSON file. On failure, create a new one.
    /// </summary>
    /// <typeparam name="T">A reference type which has a default constructor.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
    /// <param name="logger">If supplied, this will be used to log useful log messages about the depersistance operation.</param>
    /// <param name="rethrowDeserializationExceptions">If true, any deserialization exception will be rethrown. Otherwise exceptions will be logged and a new object will be returned.</param>
    /// <param name="renameFailedFiles">If true, an file which is found but cannot be deserialized will be renamed before a default object is created.</param>
    /// <returns></returns>
    public static T Depersist<T>(string? path, out bool isNew, ILogger? logger = null, bool rethrowDeserializationExceptions = false, bool renameFailedFiles = true)
        where T : new()
        => Depersist<T>(path, null, out isNew, logger, rethrowDeserializationExceptions, renameFailedFiles);

    /// <summary>
    /// Load any Type from a JSON file. On failure, create a new one.
    /// </summary>
    /// <typeparam name="T">A reference type which has a default constructor.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="serializationBinder">A custom serialization binder to be used instead of automatic handling.</param>
    /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
    /// <param name="logger">If supplied, this will be used to log useful log messages about the depersistance operation.</param>
    /// <param name="rethrowDeserializationExceptions">If true, any deserialization exception will be rethrown. Otherwise exceptions will be logged and a new object will be returned.</param>
    /// <param name="renameFailedFiles">If true, an file which is found but cannot be deserialized will be renamed before a default object is created.</param>
    /// <returns></returns>
    public static T Depersist<T>(string? path, ISerializationBinder? serializationBinder, out bool isNew, ILogger? logger = null, bool rethrowDeserializationExceptions = false, bool renameFailedFiles = true) where T : new()
    {
        JsonSerializer ser = new()
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            DefaultValueHandling = DefaultValueHandling.Populate,
            ContractResolver = new JsonNet.DefaultContractResolver()
        };
        if (serializationBinder is null)
        {
            ser.TypeNameHandling = TypeNameHandling.Auto;
            ser.Converters.Add(new JsonNet.JsonConverters.TypeConverter());
            ser.SerializationBinder = DefaultSerializationBinder.Instance;
        }
        else
        {
            ser.TypeNameHandling = TypeNameHandling.Objects;
            ser.SerializationBinder = serializationBinder;
        }
        T? obj = default;
        if (path is not null && File.Exists(path))
        {
            bool renameBroken = false;
            using StreamReader sr = new(path);
            {
                using JsonReader reader = new JsonTextReader(sr);
                try
                {
                    obj = ser.Deserialize<T>(reader);
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
                        throw new Exception($"Exception while deserializing {path}", ex);
                    }
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
            isNew = true;
            logger?.LogInformation("Created new object because nothing could be deserialized from {path}", path);
        }
        else
        {
            logger?.LogInformation("Successfully deserialized from {path}", path);
            isNew = false;
        }
        return obj;
    }

    /// <summary>
    /// Persist any type to a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of object to be persisted.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="obj">Object to be persisted.</param>
    public static void Persist<T>(T obj, string? path, bool enumsAsStrings = true)
        => Persist(obj, null, path, enumsAsStrings);

    /// <summary>
    /// Persist any type to a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of object to be persisted.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="obj">Object to be persisted.</param>
    public static void Persist<T>(T obj, ISerializationBinderEx? serializationBinder, string? path, bool enumsAsStrings = true)
    {
        if (obj is null || path is null)
        {
            return;
        }
        EnsureDirectory(path);
        JsonSerializer ser = new ()
        {
            Formatting = Formatting.Indented,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            ContractResolver = new JsonNet.DefaultContractResolver()
        };
        if (serializationBinder is null)
        {
            ser.TypeNameHandling = TypeNameHandling.Auto;
            ser.Converters.Add(new JsonNet.JsonConverters.TypeConverter());
        }
        else
        {
            ser.TypeNameHandling = serializationBinder.TypeNameHandling ?? TypeNameHandling.Objects;
            ser.SerializationBinder = serializationBinder;
        }
        if (enumsAsStrings)
        {
            ser.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }
        using var sw = new StreamWriter(path);
        using JsonWriter writer = new JsonTextWriter(sw);
        ser.Serialize(writer, obj);
    }

    public static JsonNet.Persistence Persistence { get; } = new();
}
