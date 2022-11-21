// (C) Barjonas LLC 2018

namespace Barjonas.Common.Converters;
public class EnumerableToStringConverter : BaseConverters.EnumerableToStringConverter, IValueConverter
{
    public EnumerableToStringConverter() : base(Binding.DoNothing)
    {
    }
}

