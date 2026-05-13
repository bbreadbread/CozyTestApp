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

namespace Safety_Wheel.Pages.Student
{
    /// <summary>
    /// Логика взаимодействия для StudTestInfo.xaml
    /// </summary>
    public partial class StudTestInfo : Page
    {/**/
        Test test = new();
        TestType _testType = new();
        TestTypeService _typeService = new();
        public string TestTypeName { get; set; }
        public string TestName { get; set; }
        public string TimeLimit { get; set; }
        int typeTest = 1;
        public StudTestInfo(Test currentTest, int typeTest)
        {
            this.typeTest = typeTest;
            test = currentTest;
            InitializeComponent();

            _testType = _typeService.GetTypeById(typeTest);
            TestTypeName = _testType.Name;
            TestName = currentTest.Name;
            TimeLimit = GetTimeLimitDisplay();

            DataContext = this;
        }

        private void ButtonStartTest_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new StudTest(test, _testType.TimeLimitSecond, typeTest));
        }

        private string GetTimeLimitDisplay()
        {
            if (_testType?.TimeLimitSecond == null || _testType.TimeLimitSecond <= 0)
                return "Без ограничения времени";

            var timeSpan = TimeSpan.FromSeconds((double)_testType.TimeLimitSecond);

            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} ч {timeSpan.Minutes} мин";
            else if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes} мин {timeSpan.Seconds} сек";
            else
                return $"{timeSpan.Seconds} сек";
        }

    }
}
