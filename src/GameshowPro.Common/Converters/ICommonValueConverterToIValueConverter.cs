
namespace GameshowPro.Common.Converters;

public class EnumerableToStringConverter : BaseConverters.EnumerableToStringConverter, IValueConverter
{
    public EnumerableToStringConverter() : base(Binding.DoNothing) { }
}

public class EnumToDescriptionConverter : BaseConverters.EnumToDescriptionConverter, IValueConverter {}

public class PluralConverter : BaseConverters.PluralConverter, IValueConverter { }

public class AddConverter : BaseConverters.AddConverter, IValueConverter
{
    public AddConverter() : base(Binding.DoNothing) { }
}

public class DateTimeToLocalConverter : BaseConverters.DateTimeToLocalConverter, IValueConverter { }

public class EnumerableStringToObjectsConverter : BaseConverters.EnumerableStringToObjectsConverter, IValueConverter
{
    public EnumerableStringToObjectsConverter() : base(Binding.DoNothing) { }
}

public class FsToDbfs : BaseConverters.FsToDbfs, IValueConverter { }
public class IntToCharConverter : BaseConverters.IntToCharConverter, IValueConverter { }
public class IntToOrdinalConverter : BaseConverters.IntToOrdinalConverter, IValueConverter { }
public class NullableToStringConverter : BaseConverters.NullableToStringConverter, IValueConverter { }
public class NullToMinusOneConverter : BaseConverters.NullToMinusOneConverter, IValueConverter { }
public class PipeRemoverConverter : BaseConverters.PipeRemoverConverter, IValueConverter { }
public class TimeChangeKindConverter : BaseConverters.TimeChangeKindConverter, IValueConverter { }
public class TimeSpanToSecondsConverter : BaseConverters.TimeSpanToSecondsConverter, IValueConverter 
{
    public TimeSpanToSecondsConverter() : base(DependencyProperty.UnsetValue) { }
}

public class TimeSpanToSpecialFormatConverter : BaseConverters.TimeSpanToSpecialFormatConverter, IValueConverter
{
    public TimeSpanToSpecialFormatConverter() : base(DependencyProperty.UnsetValue) { }
}

public class ToOneBasedConverter : BaseConverters.ToOneBasedConverter, IValueConverter { }

public class ToIntConverter : BaseConverters.ToIntConverter, IValueConverter { }

public class UpperCaseConverter : BaseConverters.UpperCaseConverter, IValueConverter { }
public class PathPartConverter : BaseConverters.PathPartConverter, IValueConverter { }
public class StringFormatConverter : BaseConverters.StringFormatConverter, IMultiValueConverter { }
