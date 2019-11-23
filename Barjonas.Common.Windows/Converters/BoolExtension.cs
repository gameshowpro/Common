// (C) Barjonas LLC 2018

using System;
using System.Windows.Markup;

namespace Barjonas.Common.Converters
{
    public sealed class BoolExtension : MarkupExtension
    {
        public BoolExtension(bool value) { Value = value; }
        public bool Value { get; set; }
        public override object ProvideValue(IServiceProvider sp) { return Value; }
    };
}
