namespace GameshowPro.Common.JsonConverters;

/// <summary>
/// Json converter that round-trips object values with explicit type metadata.
/// </summary>
public class TypedObjectConverter : JsonConverter<object?>
{
    private const string TypeProperty = "$type";
    private const string ValueProperty = "value";

    private readonly bool _enforceRegistryAliases;
    private readonly FrozenSet<Type> _allowedTypes;

    public TypedObjectConverter() : this(false, null)
    {
    }

    internal TypedObjectConverter(bool enforceRegistryAliases, IEnumerable<Type>? allowedTypes)
    {
        _enforceRegistryAliases = enforceRegistryAliases;
        _allowedTypes = (allowedTypes ?? []).ToFrozenSet();
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected an object with $type and value properties.");
        }

        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;

        if (!root.TryGetProperty(TypeProperty, out JsonElement typeElement) || typeElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("Missing or invalid $type property.");
        }
        if (!root.TryGetProperty(ValueProperty, out JsonElement valueElement))
        {
            throw new JsonException("Missing value property.");
        }

        string typeName = typeElement.GetString() ?? throw new JsonException("$type cannot be null.");
        Type resolvedType = TypeAliasRegistry.ResolveType(typeName);
        EnsureTypeAllowed(resolvedType, "$type");

        if (resolvedType.IsTypeOrRuntimeType())
        {
            if (valueElement.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            if (valueElement.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("Type values must serialize as strings.");
            }
            string valueTypeName = valueElement.GetString() ?? throw new JsonException("Type value cannot be null.");
            return TypeAliasRegistry.ResolveType(valueTypeName);
        }

        object? result = valueElement.Deserialize(resolvedType, options);
        if (result is null && resolvedType.IsValueType && Nullable.GetUnderlyingType(resolvedType) is null)
        {
            throw new JsonException($"Type '{resolvedType.FullName}' cannot be null.");
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Type runtimeType = value.GetType();
        Type taggedType = runtimeType.IsTypeOrRuntimeType() ? typeof(Type) : runtimeType;
        EnsureTypeAllowed(taggedType, "runtime value");

        writer.WriteStartObject();
        writer.WriteString(TypeProperty, TypeAliasRegistry.GetTypeAlias(taggedType));
        writer.WritePropertyName(ValueProperty);

        if (runtimeType.IsTypeOrRuntimeType())
        {
            writer.WriteStringValue(TypeAliasRegistry.GetTypeAlias((Type)value));
        }
        else
        {
            JsonSerializer.Serialize(writer, value, runtimeType, options);
        }

        writer.WriteEndObject();
    }

    private void EnsureTypeAllowed(Type runtimeType, string context)
    {
        if (_enforceRegistryAliases && !TypeAliasRegistry.IsKnownSupportedType(runtimeType))
        {
            throw new JsonException($"Type '{runtimeType.FullName}' in {context} is not part of the known supported alias set.");
        }

        if (_allowedTypes.Count != 0 && !_allowedTypes.Contains(runtimeType))
        {
            throw new JsonException($"Type '{runtimeType.FullName}' in {context} is not in the configured allow-list.");
        }
    }
}
