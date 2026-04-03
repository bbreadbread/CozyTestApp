using CozyTest.ViewModels.CreateTestsVM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class CardWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TestListItemViewModel vm)
            {
                if (vm.IsCreateCard || vm.IsExcelCard)
                    return 175.0; 
                return 400.0;     
            }
            return 400.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class CardMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TestListItemViewModel vm)
            {
                if (vm.IsCreateCard || vm.IsExcelCard)
                    return new Thickness(25);   

                return new Thickness(25);  
            }
            return new Thickness(25);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
