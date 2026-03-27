using System.Runtime.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GameshowPro.Common;

/// <summary>
/// Handles non-public JsonConstructor patterns and optional jsonPresentProperties constructor injection.
/// </summary>
public sealed class FlexibleJsonConstructorConverterFactory : JsonConverterFactory
{
    private readonly NullabilityInfoContext _nullabilityContext = new();

    public override bool CanConvert(Type typeToConvert)
        => GetJsonConstructor(typeToConvert) is not null;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ConstructorInfo constructor = GetJsonConstructor(typeToConvert)
            ?? throw new JsonException($"No [JsonConstructor] found for {typeToConvert.FullName}.");

        Type converterType = typeof(FlexibleJsonConstructorConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, constructor, options.PropertyNamingPolicy, _nullabilityContext)!;
    }

    private static ConstructorInfo? GetJsonConstructor(Type type)
    {
        ConstructorInfo[] constructors = type
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(c => c.IsDefined(typeof(JsonConstructorAttribute), true))
            .ToArray();

        if (constructors.Length > 1)
        {
            throw new JsonException($"Multiple constructors with [JsonConstructor] found on {type.FullName}.");
        }

        return constructors.Length == 1 ? constructors[0] : null;
    }

    private sealed class FlexibleJsonConstructorConverter<T>(
        ConstructorInfo constructor,
        JsonNamingPolicy? namingPolicy,
        NullabilityInfoContext nullabilityContext) : JsonConverter<T>
    {
        private static readonly Type s_jsonPresentSetType = typeof(IReadOnlySet<string>);
        private readonly ConstructorInfo _constructor = constructor;
        private readonly ParameterInfo[] _parameters = constructor.GetParameters();
        private readonly Dictionary<string, ParameterInfo> _parametersByJsonName = constructor.GetParameters()
            .ToDictionary(p => ToJsonName(p.Name!, namingPolicy), StringComparer.OrdinalIgnoreCase);
        private readonly List<MemberMetadata> _members = GetDataMembers(typeof(T), namingPolicy, nullabilityContext);
        private readonly NullabilityInfoContext _nullabilityContext = nullabilityContext;

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Expected JSON object for {typeof(T).FullName}.");
            }

            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement root = document.RootElement;

            Dictionary<string, JsonElement> jsonProperties = new(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty property in root.EnumerateObject())
            {
                jsonProperties[property.Name] = property.Value;
            }

            object?[] args = new object?[_parameters.Length];
            FrozenSet<string> presentProperties = jsonProperties.Keys.ToFrozenSet(StringComparer.Ordinal);

            for (int i = 0; i < _parameters.Length; i++)
            {
                ParameterInfo parameter = _parameters[i];

                if (IsJsonPresentPropertiesParameter(parameter))
                {
                    args[i] = presentProperties;
                    continue;
                }

                string jsonName = ToJsonName(parameter.Name!, namingPolicy);
                if (!jsonProperties.TryGetValue(jsonName, out JsonElement element))
                {
                    args[i] = GetMissingValue(parameter);
                    continue;
                }

                args[i] = DeserializeParameterValue(parameter, element, options);
            }

            T instance;
            try
            {
                instance = (T)_constructor.Invoke(args);
            }
            catch (TargetInvocationException ex)
            {
                throw new JsonException($"Failed invoking constructor for {typeof(T).FullName}.", ex.InnerException ?? ex);
            }

            foreach (MemberMetadata member in _members)
            {
                if (member.IsConstructorParameter)
                {
                    continue;
                }
                if (!jsonProperties.TryGetValue(member.JsonName, out JsonElement element))
                {
                    continue;
                }

                object? value = element.Deserialize(member.MemberType, options);
                if (value is null && member.DisallowNull)
                {
                    throw new JsonException($"Property '{member.JsonName}' cannot be null.");
                }
                member.Set(instance!, value);
            }

            return instance;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            foreach (MemberMetadata member in _members)
            {
                writer.WritePropertyName(member.JsonName);
                JsonSerializer.Serialize(writer, member.Get(value), member.MemberType, options);
            }
            writer.WriteEndObject();
        }

        private object? DeserializeParameterValue(ParameterInfo parameter, JsonElement element, JsonSerializerOptions options)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                if (DisallowNull(parameter.ParameterType, _nullabilityContext.Create(parameter).ReadState))
                {
                    throw new JsonException($"Parameter '{parameter.Name}' cannot be null.");
                }
                return null;
            }

            object? value = element.Deserialize(parameter.ParameterType, options);
            if (value is null && DisallowNull(parameter.ParameterType, _nullabilityContext.Create(parameter).ReadState))
            {
                throw new JsonException($"Parameter '{parameter.Name}' cannot be null.");
            }
            return value;
        }

        private static bool IsJsonPresentPropertiesParameter(ParameterInfo parameter)
            => string.Equals(parameter.Name, "jsonPresentProperties", StringComparison.Ordinal)
               && s_jsonPresentSetType.IsAssignableFrom(parameter.ParameterType);

        private static object? GetMissingValue(ParameterInfo parameter)
        {
            JsonMissingDefaultAttribute? missingDefault = parameter.GetCustomAttribute<JsonMissingDefaultAttribute>(true);
            if (missingDefault is not null)
            {
                return ChangeType(missingDefault.Value, parameter.ParameterType);
            }

            if (parameter.ParameterType.IsValueType)
            {
                return Activator.CreateInstance(parameter.ParameterType);
            }

            return null;
        }

        private static object? ChangeType(object? value, Type destinationType)
        {
            if (value is null)
            {
                return null;
            }

            Type targetType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;
            if (targetType.IsInstanceOfType(value))
            {
                return value;
            }
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private static List<MemberMetadata> GetDataMembers(Type type, JsonNamingPolicy? namingPolicy, NullabilityInfoContext nullabilityContext)
        {
            List<MemberMetadata> members = [];
            HashSet<string> ctorParamNames = type
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(c => c.IsDefined(typeof(JsonConstructorAttribute), true))
                .SelectMany(c => c.GetParameters())
                .Select(p => ToJsonName(p.Name!, namingPolicy))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (PropertyInfo property in type.GetProperties(flags))
            {
                if (!property.IsDefined(typeof(DataMemberAttribute), true))
                {
                    continue;
                }
                MethodInfo? getter = property.GetGetMethod(true);
                if (getter is null)
                {
                    continue;
                }
                MethodInfo? setter = property.GetSetMethod(true);
                string jsonName = ToJsonName(property.Name, namingPolicy);
                members.Add(new MemberMetadata(
                    jsonName,
                    property.PropertyType,
                    obj => getter.Invoke(obj, null),
                    setter is null ? static (_, _) => { } : (obj, val) => setter.Invoke(obj, [val]),
                    setter is not null,
                    ctorParamNames.Contains(jsonName),
                    DisallowNull(property.PropertyType, nullabilityContext.Create(property).ReadState)));
            }

            foreach (FieldInfo field in type.GetFields(flags))
            {
                if (!field.IsDefined(typeof(DataMemberAttribute), true))
                {
                    continue;
                }
                string fieldName = field.Name.StartsWith('_') ? field.Name[1..] : field.Name;
                string jsonName = ToJsonName(fieldName, namingPolicy);
                members.Add(new MemberMetadata(
                    jsonName,
                    field.FieldType,
                    obj => field.GetValue(obj),
                    (obj, val) => field.SetValue(obj, val),
                    true,
                    ctorParamNames.Contains(jsonName),
                    DisallowNull(field.FieldType, nullabilityContext.Create(field).ReadState)));
            }

            // Ensure deterministic output and avoid duplicate json names.
            return members
                .GroupBy(m => m.JsonName, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(m => m.JsonName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string ToJsonName(string clrName, JsonNamingPolicy? namingPolicy)
            => namingPolicy?.ConvertName(clrName) ?? clrName;

        private static bool DisallowNull(Type type, NullabilityState state)
        {
            if (type.IsValueType)
            {
                return Nullable.GetUnderlyingType(type) is null;
            }
            return state == NullabilityState.NotNull;
        }

        private sealed record MemberMetadata(
            string JsonName,
            Type MemberType,
            Func<object, object?> Get,
            Action<object, object?> Set,
            bool CanSet,
            bool IsConstructorParameter,
            bool DisallowNull);
    }
}
