// (C) Barjonas LLC 2018

using System;
using System.Windows.Markup;

namespace Barjonas.Common.Converters
{
    public sealed class DoubleExtension : MarkupExtension
    {
        public DoubleExtension(double value) { Value = value; }
        public double Value { get; set; }
        public override object ProvideValue(IServiceProvider sp) { return Value; }
    };
}
