namespace GameshowPro.Common.JsonNet;

/// <summary>
/// Extends Json.NET's <see cref="Newtonsoft.Json.Serialization.ISerializationBinder"/> with an optional
/// <see cref="Newtonsoft.Json.TypeNameHandling"/> preference to guide type metadata emission.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public interface ISerializationBinderEx : Newtonsoft.Json.Serialization.ISerializationBinder
{
    /// <summary>
    /// Gets the preferred <see cref="Newtonsoft.Json.TypeNameHandling"/> to use when serializing types, or null to use defaults.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public TypeNameHandling? TypeNameHandling { get; }
}
