using MessagePack;

namespace GameshowPro.Common.Model;

[MessagePackObject]
public record TriggerDefinition([property: Key(0)] int Input, [property: Key(1)] bool RisingEdge);
