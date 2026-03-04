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
        private Participant _participant;

        public Participant Participant
        {
            get => _participant;
            set => SetProperty(ref _participant, value);
        }

        public StatisticsViewModel(Participant participant)
        {
            Participant = participant;
        }
    }
}
