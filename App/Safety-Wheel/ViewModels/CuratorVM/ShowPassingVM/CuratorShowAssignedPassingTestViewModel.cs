using CozyTest.Models;
using CozyTest.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.ShowPassingVM
{
    public class CuratorShowAssignedPassingTestViewModel : BaseViewModel
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

        public ICommand ShowCurrentAssignedCommand { get; }
        public CuratorShowAssignedPassingTestViewModel(
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

            ShowCurrentAssignedCommand = new RelayCommand(_ => _ = ShowCurrentAssigned());

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
                await _testService.GetAllAssignedAsync(true);
            else
                await _testService.GetAllAssignedAsync(false);

            foreach (var test in _testService.Tests)
            {
                if (test.Questions != null)
                    test.PenaltyMax = test.Questions.Count;

                Tests.Add(test);
            }
        }
        private async Task ShowCurrentAssigned()
        {
            var vm = ActivatorUtilities.CreateInstance<CuratorShowAssignedPassingCurrentTestViewModel>(_serviceProvider, SelectedTest);
            _dialogService.ShowWindow<ShellWindow>(vm);
        }
    }
}