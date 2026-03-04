using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.ViewModels.StatisticsVM
{
    public class StatisticsTestRowViewModel
    {
        public Models.Test Test { get; }
        public string Topic => Test.Topic?.Name;
        public string TestName => Test.Name;

        public StatisticsTestRowViewModel(Models.Test test, Models.Participant participant)
        {
            Test = test;
        }
    }

}
