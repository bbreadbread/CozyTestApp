using CozyTest.Models;
using CozyTest.Services;
using Microsoft.Extensions.DependencyInjection;
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

namespace CozyTest.Pages.Participant
{
    public partial class PartPassingTestPageInfo : Page
    {
        private readonly Test _test;
        private readonly DTestTypeService _typeService;
        private readonly IServiceProvider _serviceProvider;
        private DTestType _testType;

        public string DTestTypeName { get; set; }
        public string TestName { get; set; }
        public string TimeLimit { get; set; }

        private int _typeTest;

        public PartPassingTestPageInfo(
            Test currentTest,
            int typeTest,
            DTestTypeService typeService,
            IServiceProvider serviceProvider)
        {
            _test = currentTest;
            _typeTest = typeTest;
            _typeService = typeService;
            _serviceProvider = serviceProvider;

            InitializeComponent();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _typeService.InitializeAsync();
            _testType = _typeService.GetTypeById(_typeTest);

            DTestTypeName = _testType?.Name ?? "Неизвестный тип";
            TestName = _test.Name;
            TimeLimit = GetTimeLimitDisplay();

            DataContext = this;
        }

        private async void ButtonStartTest_Click(object sender, RoutedEventArgs e)
        {
            var PartPassingTestPage = _serviceProvider.GetRequiredService<PartPassingTestPage>();
            NavigationService.Navigate(PartPassingTestPage);
        }

        private string GetTimeLimitDisplay()
        {
            if (_test.TimeLimitSecond == null || _test.TimeLimitSecond <= 0)
                return "Без ограничения времени";

            var timeSpan = TimeSpan.FromSeconds((double)_test.TimeLimitSecond);

            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} ч {timeSpan.Minutes} мин";
            else if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes} мин {timeSpan.Seconds} сек";
            else
                return $"{timeSpan.Seconds} сек";
        }
    }
}