using System.Text.Json.Serialization.Metadata;

namespace GameshowPro.Common;

/// <summary>
/// Custom Type resolver that only includes properties which are marked with the <see cref="TDataMemberAttribute"/>.
/// </summary>
public class OptInResolver<TDataMemberAttribute> : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            var properties = typeInfo.Properties
                .Where(prop => prop.AttributeProvider?.GetCustomAttributes(typeof(TDataMemberAttribute), true).Length > 0)
                .ToList();

            typeInfo.Properties.Clear();
            foreach (JsonPropertyInfo? property in properties)
            {
                typeInfo.Properties.Add(property);
            }
        }

        return typeInfo;
    }
}
