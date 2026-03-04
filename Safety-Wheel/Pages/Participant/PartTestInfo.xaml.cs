using Safety_Wheel.Models;
using Safety_Wheel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Safety_Wheel.Pages.Participant
{
    /// <summary>
    /// Логика взаимодействия для PartTestInfo.xaml
    /// </summary>
    public partial class PartTestInfo : Page
    {/**/
        Test test = new();
        DTestType _testType = new();
        DTestTypeService _typeService = new();
        public string DTestTypeName { get; set; }
        public string TestName { get; set; }
        public string TimeLimit { get; set; }
        int typeTest = 1;
        public PartTestInfo(Test currentTest, int typeTest)
        {
            this.typeTest = typeTest;
            test = currentTest;
            InitializeComponent();

            _testType = _typeService.GetTypeById(typeTest);
            DTestTypeName = _testType.Name;
            TestName = currentTest.Name;
            TimeLimit = GetTimeLimitDisplay();

            DataContext = this;
        }

        private void ButtonStartTest_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PartTest(test, test.TimeLimitSecond));
        }

        private string GetTimeLimitDisplay()
        {
            if (test.TimeLimitSecond == null || test.TimeLimitSecond <= 0)
                return "Без ограничения времени";

            var timeSpan = TimeSpan.FromSeconds((double)test.TimeLimitSecond);

            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} ч {timeSpan.Minutes} мин";
            else if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes} мин {timeSpan.Seconds} сек";
            else
                return $"{timeSpan.Seconds} сек";
        }

    }
}
