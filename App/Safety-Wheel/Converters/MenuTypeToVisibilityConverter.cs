using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CozyTest.ViewModels;

namespace CozyTest.Converters
{
    public class MenuTypeToSingleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MainViewModel.MainMenuType selectedMenuType && parameter is string requiredMenuType)
            {
                if (selectedMenuType.ToString() == requiredMenuType)
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}