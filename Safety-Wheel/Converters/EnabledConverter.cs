using CozyTest.Models;
using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class EnabledConverter : IMultiValueConverter
    {
        GroupService service = new();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is not Group group)
                return false;

            var list = service.GetAllParticipantForGroup(group.Id);
            return !list.Any(); 
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
