using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Safety_Wheel.ViewModels;

namespace Safety_Wheel.Converters
{
    public class MenuTypeToSingleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MainViewModelCurator.MainMenuType selectedMenuType && parameter is string requiredMenuType)
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

    public class MenuTypeToMultiVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is MainViewModelCurator.MainMenuType selectedMenuType &&
                values[1] is int itemsCount)
            {
                if (parameter is string menuName)
                {
                    if (menuName == "Participants")
                    {
                        if ((selectedMenuType == MainViewModelCurator.MainMenuType.TestResults ||
                             selectedMenuType == MainViewModelCurator.MainMenuType.Statistics) &&
                            itemsCount > 0)
                        {
                            return Visibility.Visible;
                        }
                    }
                    else if (menuName == "Dates" || menuName == "Attempts")
                    {
                        if (selectedMenuType == MainViewModelCurator.MainMenuType.TestResults &&
                            itemsCount > 0)
                        {
                            return Visibility.Visible;
                        }
                    }
                }
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}