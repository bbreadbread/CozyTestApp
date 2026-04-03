using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class RoleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int currentRole = CurrentUser.TypeUser;

            if (parameter == null)
                return Visibility.Collapsed;

            string[] allowedRoles = parameter.ToString().Split(',');

            foreach (var role in allowedRoles)
            {
                var trimmedRole = role.Trim();

                bool isMatch = trimmedRole switch
                {
                    "Admin" => currentRole == 1,
                    "Curator" => currentRole == 2,
                    "All" => true,
                    _ => false
                };

                if (isMatch)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PasswordEditVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int currentRole = CurrentUser.TypeUser;

            if (parameter == null)
                return Visibility.Collapsed;

            string[] allowedRoles = parameter.ToString().Split(',');

            foreach (var role in allowedRoles)
            {
                var trimmedRole = role.Trim();

                bool isMatch = trimmedRole switch
                {
                    "Admin" => currentRole == 1,
                    "Curator" => currentRole == 2,
                    "All" => true,
                    _ => false
                };

                if (isMatch)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
