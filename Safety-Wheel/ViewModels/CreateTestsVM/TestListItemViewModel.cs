using Safety_Wheel.Models;
using Safety_Wheel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.ViewModels.CreateTestsVM
{
    public class TestListItemViewModel : ObservableObject
    {
        private TestService _testService;
        public Test? Test { get; } = new();
        public bool IsCreateCard { get; }
        public bool IsExcelCard { get; }

        public TestListItemViewModel(Test test, TestService testService)
        {
            _testService = testService;
            Test = test;
            IsCreateCard = false;
            IsExcelCard = false;
        }

        public TestListItemViewModel(bool q = false)
        {
            if (q == true)
            {
                IsCreateCard = true;
                IsExcelCard = false;
            }
            else
            {
                IsCreateCard = true;
                IsExcelCard = true;
            }
        }

        public bool? IsPublic
        {
            get => Test.IsPublic;
            set
            {
                Test.IsPublic = value;
                OnPropertyChanged();
                _testService.Update(Test);
            }
        }
    }
}


