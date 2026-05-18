using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public string DefaultColor { get; set; } = "#666666";
        public string Mapping { get; set; } = ""; 

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return DefaultColor;

            var map = Mapping.Split(';')
                .Select(x => x.Split('='))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x[0], x => x[1]);

            return map.TryGetValue(value.ToString(), out var color) ? color : DefaultColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
