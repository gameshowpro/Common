namespace GameshowPro.Common.JsonNet;

public interface ISerializationBinderEx : Newtonsoft.Json.Serialization.ISerializationBinder
{
    public TypeNameHandling? TypeNameHandling { get; }
}
