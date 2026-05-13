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
using CozyTest.ViewModels.CuratorVM.ShowPassingVM;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CuratorWelcomePageViewModel : BaseViewModel
    {
        private readonly RequestService _requestService;
        private readonly IServiceProvider _serviceProvider;

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

        public CuratorWelcomePageViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            RequestService requestService,
            IServiceProvider serviceProvider) : base(navigationService, dialogService)
        {
            _requestService = requestService;
            _serviceProvider = serviceProvider;

            GoTestCommand = new RelayCommand(_ => GoTest());
            GoResultCommand = new RelayCommand(_ => GoResult());
            GoStatisticCommand = new RelayCommand(_ => GoStatistic());

            GoRapticipantCommand = new RelayCommand(_ => GoRapticipant());
            GoGroupCommand = new RelayCommand(_ => GoGroup());
            GoRequestCommand = new RelayCommand(_ => GoRequest());
            GoCuratorCommand = new RelayCommand(_ => GoCurator());

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _requestService.InitializeAsync();
            RequestCount = _requestService.GetAllActive().Count();
        }

        public void GoTest() => _navigationService.NavigateTo(_serviceProvider.GetRequiredService<CuratorAllTestViewModel>());
        public void GoResult() => _navigationService.NavigateTo(_serviceProvider.GetRequiredService<CuratorShowPassingTestsViewModel>());
        public void GoStatistic() => _navigationService.NavigateTo(_serviceProvider.GetRequiredService<CuratorAllTestViewModel>());

        public void GoRapticipant() => _navigationService.NavigateTo(_serviceProvider.GetRequiredService<ParticipantsViewModel>());
        public void GoGroup() => _navigationService.NavigateTo(_serviceProvider.GetRequiredService<GroupsViewModel>());
        public void GoRequest() => _navigationService.NavigateTo(_serviceProvider.GetRequiredService<RequestsViewModel>());
        public void GoCurator() => _navigationService.NavigateTo(_serviceProvider.GetRequiredService<CuratorsViewModel>());
    }
}