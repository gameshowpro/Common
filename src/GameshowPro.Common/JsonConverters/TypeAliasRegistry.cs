namespace GameshowPro.Common.JsonConverters;

/// <summary>
/// Encodes CLR types to compact aliases for persisted JSON and resolves aliases back to CLR types.
/// </summary>
public static class TypeAliasRegistry
{
    private static readonly Lock s_contextLock = new();

    private static readonly FrozenDictionary<string, Type> s_aliasToType =
        new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            ["i8"] = typeof(sbyte),
            ["u8"] = typeof(byte),
            ["i16"] = typeof(short),
            ["u16"] = typeof(ushort),
            ["i32"] = typeof(int),
            ["u32"] = typeof(uint),
            ["i64"] = typeof(long),
            ["u64"] = typeof(ulong),
            ["ch"] = typeof(char),
            ["f32"] = typeof(float),
            ["f64"] = typeof(double),
            ["dec"] = typeof(decimal),
            ["bool"] = typeof(bool),
            ["str"] = typeof(string),
            ["dt"] = typeof(DateTime),
            ["ts"] = typeof(TimeSpan),
            ["type"] = typeof(Type)
        }.ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly FrozenDictionary<Type, string> s_typeToAlias =
        s_aliasToType.ToDictionary(static kvp => kvp.Value, static kvp => kvp.Key).ToFrozenDictionary();

    private static FrozenDictionary<string, Type> s_relativeNameToType =
        new Dictionary<string, Type>(StringComparer.Ordinal).ToFrozenDictionary(StringComparer.Ordinal);

    private static FrozenDictionary<Type, string> s_typeToRelativeName =
        new Dictionary<Type, string>().ToFrozenDictionary();

    /// <summary>
    /// Configure relative-name lookup for a specific assembly context.
    /// Type names inside this assembly can be serialized as short names relative to <paramref name="rootNamespace"/>.
    /// </summary>
    public static void ConfigureRelativeTypeMap(Assembly assembly, string? rootNamespace = null)
    {
        rootNamespace ??= assembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(rootNamespace))
        {
            lock (s_contextLock)
            {
                s_relativeNameToType = new Dictionary<string, Type>(StringComparer.Ordinal).ToFrozenDictionary(StringComparer.Ordinal);
                s_typeToRelativeName = new Dictionary<Type, string>().ToFrozenDictionary();
            }
            return;
        }

        string prefix = rootNamespace + ".";
        Dictionary<string, Type> relativeLookup = new(StringComparer.Ordinal);
        Dictionary<Type, string> reverseLookup = [];

        foreach (TypeInfo typeInfo in assembly.DefinedTypes)
        {
            string? fullName = typeInfo.FullName;
            if (fullName?.StartsWith(prefix, StringComparison.Ordinal) != true)
            {
                continue;
            }

            string relative = fullName[prefix.Length..];
            if (!relativeLookup.ContainsKey(relative))
            {
                relativeLookup.Add(relative, typeInfo.AsType());
            }

            if (!reverseLookup.ContainsKey(typeInfo.AsType()))
            {
                reverseLookup.Add(typeInfo.AsType(), relative);
            }
        }

        lock (s_contextLock)
        {
            s_relativeNameToType = relativeLookup.ToFrozenDictionary(StringComparer.Ordinal);
            s_typeToRelativeName = reverseLookup.ToFrozenDictionary();
        }
    }

    /// <summary>
    /// Convert a CLR type to a compact alias string for persistence.
    /// </summary>
    public static string GetTypeAlias(Type type)
    {
        if (s_typeToAlias.TryGetValue(type, out string? primitiveAlias))
        {
            return primitiveAlias;
        }

        Type? nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying != null)
        {
            return $"n({GetTypeAlias(nullableUnderlying)})";
        }

        if (type.IsEnum)
        {
            return $"e({EncodeRuntimeType(type)})";
        }

        if (type.IsGenericType)
        {
            Type genericDefinition = type.GetGenericTypeDefinition();
            Type[] args = type.GetGenericArguments();

            if (genericDefinition == typeof(ImmutableArray<>))
            {
                return $"ia({GetTypeAlias(args[0])})";
            }
            if (genericDefinition == typeof(ImmutableList<>))
            {
                return $"il({GetTypeAlias(args[0])})";
            }
            if (genericDefinition == typeof(ImmutableHashSet<>))
            {
                return $"ih({GetTypeAlias(args[0])})";
            }
            if (genericDefinition == typeof(ImmutableDictionary<,>))
            {
                return $"id({GetTypeAlias(args[0])},{GetTypeAlias(args[1])})";
            }
        }

        return EncodeRuntimeType(type);
    }

    /// <summary>
    /// Resolve a persisted alias (or legacy assembly-qualified name) to a CLR type.
    /// </summary>
    public static Type ResolveType(string persistedTypeName)
    {
        if (string.IsNullOrWhiteSpace(persistedTypeName))
        {
            throw new JsonException("Type alias cannot be null or whitespace.");
        }

        if (TryParseAlias(persistedTypeName, out Type? parsed))
        {
            return parsed;
        }

        // Backward compatibility with existing persisted values.
        return Type.GetType(persistedTypeName)
            ?? throw new JsonException($"Could not resolve type alias or name '{persistedTypeName}'.");
    }

    /// <summary>
    /// Returns true when the type belongs to the finite built-in alias set (including supported immutable generic compositions and enums).
    /// </summary>
    public static bool IsKnownSupportedType(Type type)
    {
        if (s_typeToAlias.ContainsKey(type))
        {
            return true;
        }

        Type? nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying != null)
        {
            return IsKnownSupportedType(nullableUnderlying);
        }

        if (type.IsEnum)
        {
            return true;
        }

        if (!type.IsGenericType)
        {
            return false;
        }

        Type genericDefinition = type.GetGenericTypeDefinition();
        Type[] args = type.GetGenericArguments();

        if (genericDefinition == typeof(ImmutableArray<>)
            || genericDefinition == typeof(ImmutableList<>)
            || genericDefinition == typeof(ImmutableHashSet<>))
        {
            return IsKnownSupportedType(args[0]);
        }

        if (genericDefinition == typeof(ImmutableDictionary<,>))
        {
            return IsKnownSupportedType(args[0]) && IsKnownSupportedType(args[1]);
        }

        return false;
    }

    private static bool TryParseAlias(string input, [NotNullWhen(true)] out Type? type)
    {
        int index = 0;
        bool parsed = TryParseAlias(input, ref index, out type) && index == input.Length;
        if (!parsed)
        {
            type = null;
        }

        return parsed;
    }

    private static bool TryParseAlias(string input, ref int index, [NotNullWhen(true)] out Type? type)
    {
        int tokenStart = index;
        while (index < input.Length && char.IsLetterOrDigit(input[index]))
        {
            index++;
        }

        if (tokenStart == index)
        {
            type = null;
            return false;
        }

        string token = input[tokenStart..index];

        if (index < input.Length && input[index] == '(')
        {
            index++; // consume '('
            type = null;
            bool success = token switch
            {
                "n" => ParseNullable(input, ref index, out type),
                "ia" => ParseSingleArgGeneric(input, ref index, typeof(ImmutableArray<>), out type),
                "il" => ParseSingleArgGeneric(input, ref index, typeof(ImmutableList<>), out type),
                "ih" => ParseSingleArgGeneric(input, ref index, typeof(ImmutableHashSet<>), out type),
                "id" => ParseDictionary(input, ref index, out type),
                "e" => ParseEnum(input, ref index, out type),
                "r" => ParseRuntimeType(input, ref index, out type),
                _ => false
            };

            if (!success)
            {
                type = null;
                return false;
            }

            if (index >= input.Length || input[index] != ')')
            {
                type = null;
                return false;
            }

                if (type == null)
                {
                    return false;
                }

            index++; // consume ')'
            return true;
        }

        if (s_aliasToType.TryGetValue(token, out Type? primitiveType) && primitiveType != null)
        {
            type = primitiveType;
            return true;
        }

        type = null;
        return false;
    }

    private static bool ParseNullable(string input, ref int index, [NotNullWhen(true)] out Type? type)
    {
        if (!TryParseAlias(input, ref index, out Type? underlying))
        {
            type = null;
            return false;
        }

        if (!underlying.IsValueType || Nullable.GetUnderlyingType(underlying) != null)
        {
            type = null;
            return false;
        }

        type = typeof(Nullable<>).MakeGenericType(underlying);
        return true;
    }

    private static bool ParseSingleArgGeneric(string input, ref int index, Type genericDefinition, [NotNullWhen(true)] out Type? type)
    {
        if (!TryParseAlias(input, ref index, out Type? argType))
        {
            type = null;
            return false;
        }

        type = genericDefinition.MakeGenericType(argType);
        return true;
    }

    private static bool ParseDictionary(string input, ref int index, [NotNullWhen(true)] out Type? type)
    {
        if (!TryParseAlias(input, ref index, out Type? keyType))
        {
            type = null;
            return false;
        }

        if (index >= input.Length || input[index] != ',')
        {
            type = null;
            return false;
        }

        index++; // consume ','

        if (!TryParseAlias(input, ref index, out Type? valueType))
        {
            type = null;
            return false;
        }

        type = typeof(ImmutableDictionary<,>).MakeGenericType(keyType, valueType);
        return true;
    }

    private static bool ParseEnum(string input, ref int index, [NotNullWhen(true)] out Type? type)
    {
        if (!TryParseAlias(input, ref index, out Type? enumType) || !enumType.IsEnum)
        {
            type = null;
            return false;
        }

        type = enumType;
        return true;
    }

    private static bool ParseRuntimeType(string input, ref int index, [NotNullWhen(true)] out Type? type)
    {
        int payloadStart = index;
        while (index < input.Length && input[index] != ')')
        {
            index++;
        }

        if (payloadStart == index)
        {
            type = null;
            return false;
        }

        string payload = input[payloadStart..index];

        if (s_relativeNameToType.TryGetValue(payload, out Type? relativeType) && relativeType != null)
        {
            type = relativeType;
            return true;
        }

        type = Type.GetType(payload);
        return type != null;
    }

    private static string EncodeRuntimeType(Type type)
    {
        if (s_typeToRelativeName.TryGetValue(type, out string? relativeName))
        {
            return $"r({relativeName})";
        }

        string typeName = IsolateAssemblyAndTypeName(type)
            ?? throw new JsonException($"Cannot encode null type name for '{type}'.");
        return $"r({typeName})";
    }
}
