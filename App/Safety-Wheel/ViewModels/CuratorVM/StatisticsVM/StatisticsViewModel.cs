using CozyTest;
using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using CozyTest.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static OfficeOpenXml.ExcelErrorValue;
using System.Windows.Navigation;
using CozyTest.ViewModels.CuratorVM.ShowPassingVM;
using Microsoft.Extensions.DependencyInjection;

namespace CozyTest.ViewModels.CuratorVM.StatisticsVM
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly TestService _testService;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<Test> Tests { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        private Test _selectedTest;
        public Test SelectedTest
        {
            get => _selectedTest;
            set
            {
                _selectedTest = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowCurrentStatisticCommand { get; }
        public StatisticsViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            TestService testService,
            IServiceProvider serviceProvider) : base(navigationService, dialogService)
        {

            CurrentUser.AdminModeOnChanged += async (_, _) =>
            {
                OnPropertyChanged(nameof(AdminModeOn));
                await LoadTestsAsync();
            };

            _testService = testService;
            _serviceProvider = serviceProvider;

            ShowCurrentStatisticCommand = new RelayCommand(_ => _ = ShowCurrentStatistic());

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadTestsAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка инициализации: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTestsAsync()
        {
            Tests.Clear();

            if (CurrentUser.TypeUser == 1 && AdminModeOn == true)
                await _testService.GetAllAsync();
            else
                await _testService.GetAllAsync(CurrentUser.Id, setList: true);

            foreach (var test in _testService.Tests)
            {
                Tests.Add(test);
            }
        }
        private async Task ShowCurrentStatistic()
        {
            var vm = ActivatorUtilities.CreateInstance<StatisticsCurrentTestViewModel>(_serviceProvider, SelectedTest);
            _navigationService.NavigateTo(vm);
        }
    }
}