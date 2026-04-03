using CozyTest.Models;
using CozyTest.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class TestListItemViewModel : ObservableObject
    {
        public Test? Test { get; } = new();
        public bool IsCreateCard { get; }
        public bool IsExcelCard { get; }

        public TestListItemViewModel(Test test, TestService testService)
        {
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
            else if (CurrentUser.TypeUser == 3)
            {
                IsCreateCard = false;
                IsExcelCard = false;
            }
            else return;
        }
    }
}


