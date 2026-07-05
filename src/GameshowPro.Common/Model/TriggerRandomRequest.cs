using MessagePack;

namespace GameshowPro.Common.Model;

[MessagePackObject]
public record TriggerRandomRequest([property: Key(0)] IEnumerable<TriggerDefinition> Inputs, [property: Key(1)] TimeSpan MinimumTime, [property: Key(2)] TimeSpan MaximumTime);

[MessagePackObject]
public record TriggerRandomResponse();
