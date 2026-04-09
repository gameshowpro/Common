namespace GameshowPro.Common.JsonConverters;

/// <summary>
/// Optional constraint for object-typed JSON members handled by <see cref="TypedObjectConverter"/>.
/// When omitted, no additional type enforcement is applied.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class TypedObjectTypeConstraintAttribute : Attribute
{
    public TypedObjectTypeConstraintAttribute(params Type[] allowedTypes)
    {
        AllowedTypes = allowedTypes ?? [];
    }

    /// <summary>
    /// Additional concrete runtime types that are allowed for this member.
    /// </summary>
    public Type[] AllowedTypes { get; }

    /// <summary>
    /// If true, restricts values to the finite set supported by <see cref="TypeAliasRegistry.IsKnownSupportedType(Type)"/>.
    /// </summary>
    public bool EnforceRegistryAliases { get; set; }
}