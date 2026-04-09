using System.Text.Json.Serialization.Metadata;
using GameshowPro.Common.JsonConverters;

namespace GameshowPro.Common;

/// <summary>
/// Custom Type resolver that only includes properties which are marked with the <typeparamref name="TDataMemberAttribute"/>.
/// </summary>
public class OptInResolver<TDataMemberAttribute> : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            List<JsonPropertyInfo> properties = [.. typeInfo.Properties.Where(prop => prop.AttributeProvider?.GetCustomAttributes(typeof(TDataMemberAttribute), true).Length > 0)];

            foreach (JsonPropertyInfo property in properties)
            {
                ApplyTypedObjectConstraints(property, property.AttributeProvider);
            }

            // Add [DataMember]-annotated non-public properties.
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            foreach (PropertyInfo property in type.GetProperties(flags))
            {
                if (!property.IsDefined(typeof(TDataMemberAttribute), true))
                {
                    continue;
                }
                if (property.GetGetMethod(true) is null)
                {
                    continue;
                }

                JsonPropertyInfo info = typeInfo.CreateJsonPropertyInfo(property.PropertyType, ToJsonName(property.Name, options.PropertyNamingPolicy));
                info.Get = obj => property.GetValue(obj);
                if (property.GetSetMethod(true) is not null)
                {
                    info.Set = (obj, value) => property.SetValue(obj, value);
                }
                ApplyTypedObjectConstraints(info, property);
                properties.Add(info);
            }

            // Add [DataMember]-annotated non-public fields.
            foreach (FieldInfo field in type.GetFields(flags))
            {
                if (!field.IsDefined(typeof(TDataMemberAttribute), true))
                {
                    continue;
                }

                string fieldName = field.Name.StartsWith('_') ? field.Name[1..] : field.Name;
                JsonPropertyInfo info = typeInfo.CreateJsonPropertyInfo(field.FieldType, ToJsonName(fieldName, options.PropertyNamingPolicy));
                info.Get = obj => field.GetValue(obj);
                info.Set = (obj, value) => field.SetValue(obj, value);
                ApplyTypedObjectConstraints(info, field);
                properties.Add(info);
            }

            typeInfo.Properties.Clear();
            foreach (JsonPropertyInfo property in properties
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First()))
            {
                typeInfo.Properties.Add(property);
            }
        }

        return typeInfo;
    }

    private static string ToJsonName(string name, JsonNamingPolicy? namingPolicy)
        => namingPolicy?.ConvertName(name) ?? name;

    private static void ApplyTypedObjectConstraints(JsonPropertyInfo propertyInfo, ICustomAttributeProvider? attributeProvider)
    {
        if (propertyInfo.PropertyType != typeof(object) || attributeProvider is null)
        {
            return;
        }

        TypedObjectTypeConstraintAttribute? constraint = attributeProvider
            .GetCustomAttributes(typeof(TypedObjectTypeConstraintAttribute), true)
            .OfType<TypedObjectTypeConstraintAttribute>()
            .FirstOrDefault();

        if (constraint is null)
        {
            return;
        }

        propertyInfo.CustomConverter = new TypedObjectConverter(constraint.EnforceRegistryAliases, constraint.AllowedTypes);
    }
}
