// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Converters;

public sealed class DoubleExtension(double value) : MarkupExtension
{
    public double Value { get; set; } = value;

    public override object ProvideValue(IServiceProvider sp)
        => Value;
}
