// (C) Barjonas LLC 2018

using System.Windows.Data;
#nullable enable
namespace Barjonas.Common.Converters;
public class EnumerableToStringConverter : BaseConverters.EnumerableToStringConverter, IValueConverter
{
    public EnumerableToStringConverter() : base(Binding.DoNothing)
    {
    }
}

#nullable restore
