using CozyTest.Models;
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

            if (currentRole == 0)
            {
                return Visibility.Collapsed;
            }

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
                    "AllCurators" => currentRole == 1 || currentRole == 2,
                    "All" => currentRole == 1 || currentRole == 2 || currentRole == 3,
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
    public class RoleDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Curator curator)
                if ((bool)curator.IsAdmin) return $"Экзаменатор (Админ): {curator.Name}";
                else return $"Экзаменатор: {curator.Name}";
               
            if (value is Participant participant)
                return $"Тестируемый: {participant.Name}";
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
