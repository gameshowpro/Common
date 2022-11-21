// (C) Barjonas LLC 2018

using System.Windows.Markup;

namespace Barjonas.Common.Converters;

public sealed class Int32Extension : MarkupExtension
{
    public Int32Extension(int value) { Value = value; }
    public int Value { get; set; }
    public override object ProvideValue(IServiceProvider sp) { return Value; }
};
