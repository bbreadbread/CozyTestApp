using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class InstanceOfTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            Type targetTypeParam = parameter as Type;
            if (targetTypeParam == null)
            {
                if (parameter is string typeName)
                {
                    targetTypeParam = Type.GetType(typeName);
                    if (targetTypeParam == null)
                        return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }

            return targetTypeParam.IsInstanceOfType(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}