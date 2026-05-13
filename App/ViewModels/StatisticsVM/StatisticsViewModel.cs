using Safety_Wheel.Models;
using Safety_Wheel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.ViewModels.StatisticsVM
{
    public class StatisticsViewModel : ObservableObject
    {
        private Student _student;

        public Student Student
        {
            get => _student;
            set => SetProperty(ref _student, value);
        }

        public StatisticsViewModel(Student student)
        {
            Student = student;
        }
    }
}
