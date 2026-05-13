using CozyTest.ViewModels.CreateTestsVM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CozyTest.Converters
{
    public class QuestionToTagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedQuestion = value as QuestionCreateViewModel;
            var currentQuestion = parameter as QuestionCreateViewModel;

            if (selectedQuestion != null && currentQuestion != null && selectedQuestion == currentQuestion)
                return "Selected";

            return "NotSelected";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
