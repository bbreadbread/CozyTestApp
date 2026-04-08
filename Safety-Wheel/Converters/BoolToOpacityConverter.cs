using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? 0.4 : 1.0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }



    public class EmptyCollectionToVisibilityConverter : IValueConverter
    {
        public Visibility EmptyValue { get; set; } = Visibility.Visible;
        public Visibility NotEmptyValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = value switch
            {
                int i => i,
                ICollection<Type> col => col.Count,
                IEnumerable<Type> enumerable => enumerable.Cast<object>().Count(),
                _ => 0
            };
            return count == 0 ? EmptyValue : NotEmptyValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class BoolToGridLengthConverter : IValueConverter
    {
        public GridLength TrueValue { get; set; } = GridLength.Auto;
        public GridLength FalseValue { get; set; } = new GridLength(0);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? TrueValue : FalseValue;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    [ValueConversion(typeof(int), typeof(Visibility))]
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public Visibility ZeroValue { get; set; } = Visibility.Visible;
        public Visibility NonZeroValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = value switch
            {
                int i => i,
                null => 0,
                _ => 0
            };

            return count == 0 ? ZeroValue : NonZeroValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BoolToIntConverter : IValueConverter
    {
        public int TrueValue { get; set; } = 2;
        public int FalseValue { get; set; } = 1;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? TrueValue : FalseValue;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToTextConverter : IValueConverter
    {
        public string TrueText { get; set; } = "Да";
        public string FalseText { get; set; } = "Нет";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? TrueText : FalseText;

            return FalseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
