using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CozyTest.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class RoleLogsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string selectedRole = value.ToString();
            string targetRoles = parameter.ToString(); 

            var allowedRoles = targetRoles.Split(',')
                .Select(r => r.Trim())
                .ToList();

            string normalizedRole = selectedRole switch
            {
                "Куратор" => "Curator",
                "Куратор-администратор" => "Admin",
                "Тестируемый" => "Participant",
                "Все" => "All",
                _ => selectedRole
            };

            bool isVisible = allowedRoles.Any(r =>
                r.Equals(normalizedRole, StringComparison.OrdinalIgnoreCase) ||
                r.Equals(selectedRole, StringComparison.OrdinalIgnoreCase) ||
                (r == "All" && normalizedRole == "All"));

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}