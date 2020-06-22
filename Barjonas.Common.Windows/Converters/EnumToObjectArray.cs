// (C) Barjonas LLC 2018

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Barjonas.Common.Converters
{
    public class EnumToObjectArray : MarkupExtension, IValueConverter
    {
        public BindingBase SourceEnum { get; set; }

        private readonly Type _type;

        public EnumToObjectArray()
        {
        }

        public EnumToObjectArray(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Type typeToUse;
            if (_type == null)
            {
                DependencyObject targetObject;
                DependencyProperty targetProperty;

                if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target && target.TargetObject is DependencyObject && target.TargetProperty is DependencyProperty)
                {
                    targetObject = (DependencyObject)target.TargetObject;
                    targetProperty = (DependencyProperty)target.TargetProperty;
                }
                else
                {
                    return this;
                }

               BindingOperations.SetBinding(targetObject, EnumToObjectArray.s_sourceEnumBindingSinkProperty, SourceEnum);
                typeToUse = targetObject.GetValue(s_sourceEnumBindingSinkProperty).GetType();
                if (typeToUse.BaseType != typeof(Enum))
                {
                    return null;
                }
            }
            else
            {
                typeToUse = _type;
            }

            return TypeToList(typeToUse);
        }

        private static object TypeToList(Type type)
        {
            if (type == null)
            {
                return null;
            }
            return Enum.GetValues(type)
                            .Cast<Enum>()
                            .Select(e => new { Value = e, Name = e.ToString(), DisplayName = e.Description(), Underlying = e.UnderlyingValue() });
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TypeToList((Type)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static readonly DependencyProperty s_sourceEnumBindingSinkProperty = DependencyProperty.RegisterAttached("SourceEnumBindingSink", typeof(Enum)
                           , typeof(EnumToObjectArray), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
    }
}
