// (C) Barjonas LLC 2018

using System.Windows.Markup;

namespace Barjonas.Common.Converters;

public sealed class DecimalExtension : MarkupExtension
{
    public DecimalExtension(decimal value) { Value = value; }
    public decimal Value { get; set; }
    public override object ProvideValue(IServiceProvider sp) { return Value; }
};
