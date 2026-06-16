using ScottPlot.Plottables.Interactive;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class MarkLvlToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return App.Current.Resources[""];

            if (int.TryParse(value.ToString(), out int mark))
            {
                switch (mark)
                {
                    case 5:
                        return App.Current.Resources["LightGreen"];
                    case 4:
                        return App.Current.Resources["LightGold"];
                    case 3:
                        return App.Current.Resources["LightOrange"];
                    case 2:
                        return App.Current.Resources["LightRed"];
                    case 1:
                        return App.Current.Resources["LightTurquoise"];
                    default:
                        return App.Current.Resources["Transparent"];
                }
            };
            return App.Current.Resources[""];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
