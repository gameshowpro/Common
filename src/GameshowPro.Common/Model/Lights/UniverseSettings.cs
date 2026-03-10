// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Model.Lights;

public class UniverseSettings : ObservableClass
{
    /// <summary>
    /// Ultimately this gets sent out as a <see cref="short"/>, so value must be within signed 16-bit range.
    /// Also, it doesn't seem that many other devices support universes less than 1, so zero represents disable.
    /// </summary>
    [DataMember, DefaultValue(1)]
    public int UniverseIndex
    {
        get;
        set { SetProperty(ref field, value.KeepInRange(0, short.MaxValue)); }
    } = 1;

    /// <summary>
    /// The size of universe required. This is not (de)persisted, but written by client code after deserialization.
    /// </summary>
    public int Size
    {
        get;
        set { SetProperty(ref field, value); }
    } = 512;
}
