using CozyTest.Models;
using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.ViewModels.CreateTestsVM
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

        public TestListItemViewModel(bool? q)
        {
            if (q == true)
            {
                IsCreateCard = true;
                IsExcelCard = false;
            }
            else if (q == false)
            {
                IsCreateCard = true;
                IsExcelCard = true;
            }
            else return;
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


