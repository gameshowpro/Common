namespace GameshowPro.Common.Model;

public interface ISerializationBinderEx : Newtonsoft.Json.Serialization.ISerializationBinder
{
    public TypeNameHandling? TypeNameHandling { get; }
}
