using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class PercentageToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage && parameter is string maxWidthStr
                && double.TryParse(maxWidthStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double maxWidth))
            {
                return (percentage / 100.0) * maxWidth;
            }

            if (value is double p && parameter is double mw)
            {
                return (p / 100.0) * mw;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
