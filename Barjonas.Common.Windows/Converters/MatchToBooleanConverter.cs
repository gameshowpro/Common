// (C) Barjonas LLC 2018

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    /// <summary>
    /// Return a visibility or boolean depending on match.
    /// </summary>
    public class MatchToBooleanConverter : BaseConverters.MatchToBooleanConverter, IValueConverter, IMultiValueConverter
    {
        public MatchToBooleanConverter() : base(Binding.DoNothing) { }

        protected override object BooleanToType(bool value, Type type)
        {
            if (Invert)
            {
                value ^= true;
            }

            if (type != typeof(Visibility))
            {
                return value;
            }
            if (value)
            {
                return Visibility.Visible;
            }
            else
            {
                return FalseVisibility;
            }
        }

        public bool Invert { get; set; }
        public Visibility FalseVisibility { get; set; } = Visibility.Hidden;
    }
}
