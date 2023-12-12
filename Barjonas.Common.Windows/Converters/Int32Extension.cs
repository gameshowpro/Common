// (C) Barjonas LLC 2018

using System.Windows.Markup;

namespace Barjonas.Common.Converters;

public sealed class Int32Extension(int value) : MarkupExtension
{
    public int Value { get; set; } = value; public override object ProvideValue(IServiceProvider sp) { return Value; }
};
