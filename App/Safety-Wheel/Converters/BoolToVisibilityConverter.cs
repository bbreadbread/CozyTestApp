using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class QuestionTypeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int questionType && parameter is string targetTypeStr)
            {
                return targetTypeStr switch
                {
                    "Options" or "1" => questionType == 1 ? Visibility.Visible : Visibility.Collapsed,
                    "Text" or "2" => questionType == 2 ? Visibility.Visible : Visibility.Collapsed,
                    "Compliance" or "3" => questionType == 3 ? Visibility.Visible : Visibility.Collapsed,
                    "12" => questionType == 1 || questionType == 2 ? Visibility.Visible : Visibility.Collapsed,
                    "Empty" => questionType == 3 ? Visibility.Visible : Visibility.Collapsed,
                    _ => Visibility.Collapsed
                };
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
