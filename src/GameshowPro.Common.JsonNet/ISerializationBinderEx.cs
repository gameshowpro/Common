#pragma warning disable IDE0130 // Namespace does not match folder: this type is published in this namespace; changing it to match the project root namespace or folder would break consumers and type names stored in settings files.
namespace GameshowPro.Common.JsonNet;
#pragma warning restore IDE0130

/// <summary>
/// Extends Json.NET's <see cref="ISerializationBinder"/> with an optional
/// <see cref="Newtonsoft.Json.TypeNameHandling"/> preference to guide type metadata emission.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public interface ISerializationBinderEx : ISerializationBinder
{
    /// <summary>
    /// Gets the preferred <see cref="Newtonsoft.Json.TypeNameHandling"/> to use when serializing types, or null to use defaults.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    TypeNameHandling? TypeNameHandling { get; }
}
