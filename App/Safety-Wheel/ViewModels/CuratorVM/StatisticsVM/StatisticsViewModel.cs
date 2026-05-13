using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.ViewModels.StatisticsVM
{
    public class StatisticsViewModel : BaseViewModel
    {
        private Participant _participant;

        public StatisticsViewModel(INavigationService navigationService, IDialogService dialogService, Participant participant ) : base(navigationService, dialogService)
        {
            Participant = participant;
        }

        public Participant Participant
        {
            get => _participant;
            set => SetProperty(ref _participant, value);
        }

    }
}
