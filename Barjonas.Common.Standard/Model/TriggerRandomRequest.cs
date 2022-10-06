namespace Barjonas.Common.Model;
public record TriggerRandomRequest(IEnumerable<TriggerDefinition> Inputs, TimeSpan MinimumTime, TimeSpan MaximumTime);
public record TriggerRandomResponse();
