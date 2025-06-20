namespace GameshowPro.Common;

public class DefaultSerializationBinder : ISerializationBinderEx
{
    private readonly static FrozenDictionary<string, string> s_assemblyReplacements = new KeyValuePair<string, string>[] { KeyValuePair.Create("GameshowPro.Common.Windows", "GameshowPro.Common") }.ToFrozenDictionary();

    private DefaultSerializationBinder()
    {
    }

    public static DefaultSerializationBinder Instance { get; } = new ();

    public TypeNameHandling? TypeNameHandling => Newtonsoft.Json.TypeNameHandling.Auto;

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

    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
        assemblyName = serializedType.Assembly.GetName().Name;
        typeName = serializedType.FullName;
    }
}
