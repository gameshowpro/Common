// (C) Barjonas LLC 2018

using System.Windows.Markup;

namespace GameshowPro.Common.Converters;

public sealed class DecimalExtension(decimal value) : MarkupExtension
{
    public decimal Value { get; set; } = value; public override object ProvideValue(IServiceProvider sp) { return Value; }
};
