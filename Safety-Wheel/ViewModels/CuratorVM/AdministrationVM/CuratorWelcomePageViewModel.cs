using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using CozyTest.ViewModels.CreateTestsVM;
using Microsoft.Extensions.DependencyInjection;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CuratorWelcomePageViewModel : BaseViewModel
    {
        private RequestService requestService = new();
        public ICommand GoTestCommand { get; }
        public ICommand GoResultCommand { get; }
        public ICommand GoStatisticCommand { get; }

        public ICommand GoRapticipantCommand { get; }
        public ICommand GoGroupCommand { get; }
        public ICommand GoRequestCommand { get; }
        public ICommand GoCuratorCommand { get; }

        private int _requestCount;

        public int RequestCount
        {
            get => _requestCount;
            set => SetProperty(ref _requestCount, value);
        }

        public CuratorWelcomePageViewModel(INavigationService navigationService, IDialogService dialogService) : base(navigationService, dialogService)
        {
            GoTestCommand = new RelayCommand(_ => GoTest());
            GoResultCommand = new RelayCommand(_ => GoResult());
            GoStatisticCommand = new RelayCommand(_ => GoStatistic());

            GoRapticipantCommand = new RelayCommand(_ => GoRapticipant());
            GoGroupCommand = new RelayCommand(_ => GoGroup());
            GoRequestCommand = new RelayCommand(_ => GoRequest());
            GoCuratorCommand = new RelayCommand(_ => GoCurator());

            RequestCount = requestService.GetAllActive().Count();
        }

        public void GoTest() => _navigationService.NavigateTo(new CuratorAllTestViewModel(_navigationService, _dialogService));
        public void GoResult() => _navigationService.NavigateTo(new CuratorAllTestViewModel(_navigationService, _dialogService));
        public void GoStatistic() => _navigationService.NavigateTo(new CuratorAllTestViewModel(_navigationService, _dialogService));

        public void GoRapticipant() => _navigationService.NavigateTo(new ParticipantsViewModel(_dialogService, _navigationService));
        public void GoGroup() => _navigationService.NavigateTo(new GroupsViewModel(_dialogService, _navigationService));
        public void GoRequest() => _navigationService.NavigateTo(new RequestsViewModel(_dialogService, _navigationService));
        public void GoCurator() => _navigationService.NavigateTo(new CuratorsViewModel(_dialogService, _navigationService));
    }
}
