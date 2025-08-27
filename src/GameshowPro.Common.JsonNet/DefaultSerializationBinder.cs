namespace GameshowPro.Common;

/// <summary>
/// Default implementation of <see cref="JsonNet.ISerializationBinderEx"/> that resolves types across
/// GameshowPro assemblies and prefers <see cref="TypeNameHandling.Auto"/> for type metadata.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public class DefaultSerializationBinder : ISerializationBinderEx
{
    private readonly static FrozenDictionary<string, string> s_assemblyReplacements = new KeyValuePair<string, string>[] { KeyValuePair.Create("GameshowPro.Common.Windows", "GameshowPro.Common") }.ToFrozenDictionary();

    private DefaultSerializationBinder()
    {
    }

    /// <summary>
    /// Gets a shared singleton instance.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public static DefaultSerializationBinder Instance { get; } = new ();

    /// <summary>
    /// Gets the preferred <see cref="TypeNameHandling"/> mode.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public TypeNameHandling? TypeNameHandling => Newtonsoft.Json.TypeNameHandling.Auto;

    /// <summary>
    /// Resolves a CLR <see cref="Type"/> from Json.NET type metadata.
    /// </summary>
    /// <param name="assemblyName">The recorded assembly name.</param>
    /// <param name="typeName">The recorded type full name.</param>
    /// <returns>The resolved <see cref="Type"/>.</returns>
    /// <exception cref="JsonSerializationException">Thrown when the type cannot be resolved.</exception>
    /// <remarks>Docs added by AI.</remarks>
    public Type BindToType(string? assemblyName, string? typeName)
    {

        if (assemblyName != null && s_assemblyReplacements.TryGetValue(assemblyName, out string? newValue))
        {
            assemblyName = newValue;
        }

        if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
        {
            try
            {
                var type = Type.GetType($"{typeName}, {assemblyName}", throwOnError: false);
                if (type != null)
                    return type;
            }
            catch { }
        }

        // Fallback: try just the type name (may work for types in the current assembly)
        if (!string.IsNullOrEmpty(typeName))
        {
            var type = Type.GetType(typeName, throwOnError: false);
            if (type != null)
                return type;
        }

        throw new JsonSerializationException($"Cannot find type: {typeName}, assembly: {assemblyName}");
    }

    /// <summary>
    /// Produces Json.NET type metadata for a given <see cref="Type"/>.
    /// </summary>
    /// <param name="serializedType">The type being serialized.</param>
    /// <param name="assemblyName">Outputs the assembly name to record.</param>
    /// <param name="typeName">Outputs the type full name to record.</param>
    /// <remarks>Docs added by AI.</remarks>
    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
        assemblyName = serializedType.Assembly.GetName().Name;
        typeName = serializedType.FullName;
    }
}
