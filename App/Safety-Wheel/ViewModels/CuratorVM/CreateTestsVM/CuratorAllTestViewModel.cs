using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
using System.Windows;
using CozyTest.ViewModels.CuratorVM;
using System.Windows.Input;
using System.Windows.Navigation;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using CozyTest.ViewModels.CuratorVM.CreateTestsVM;
using System.ComponentModel;
using CozyTest.ForShellWindow;
using Microsoft.Extensions.DependencyInjection;
using WPFCustomMessageBox;

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class CuratorAllTestViewModel : BaseViewModel
    {
        private readonly TestService _testService;
        private readonly TopicService _topicService;
        private readonly CuratorService _curatorService;
        private readonly ParticipantService _participantService;
        private readonly GroupService _groupService;
        private readonly ParticipantPublicTestService _participantPublicTestService;
        private readonly IServiceProvider _serviceProvider;
        private List<Test> _allTests = new();

        public ObservableCollection<TestListItemViewModel> Tests { get; } = new();
        public ObservableCollection<Topic> Topics { get; } = new();
        public ObservableCollection<Curator> Curators { get; } = new();

        private bool _isOwnAuthorship = true;
        public bool IsOwnAuthorship
        {
            get => _isOwnAuthorship;
            set { _isOwnAuthorship = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private bool _isMyAuthorshipWith;
        public bool IsNotMyAuthorship
        {
            get => _isMyAuthorshipWith;
            set { _isMyAuthorshipWith = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private bool _isSelectedArchive;
        public bool IsSelectedArchive
        {
            get => _isSelectedArchive;
            set { _isSelectedArchive = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }
        private bool _isSelectedActive = true;
        public bool IsSelectedActive
        {
            get => _isSelectedActive;
            set { _isSelectedActive = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private Curator? _selectedCoauthor;
        public Curator? SelectedCoauthor
        {
            get => _selectedCoauthor;
            set { _selectedCoauthor = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private Topic? _selectedTopic;
        public Topic? SelectedTopic
        {
            get => _selectedTopic;
            set { _selectedTopic = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private string _testNameFilter = "";
        public string TestNameFilter
        {
            get => _testNameFilter;
            set { _testNameFilter = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private Curator? _selectedCoauthorFilter;
        public Curator? SelectedCoauthorFilter
        {
            get => _selectedCoauthorFilter;
            set { _selectedCoauthorFilter = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        public ICommand CardClickCommand { get; }
        public ICommand ArchiveTestCommand { get; }
        public ICommand AssignedTestCommand { get; }
        public ICommand CreateTestCommand { get; }
        public ICommand ImportExcelCommand { get; }
        public ICommand CreateAddTopicsCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand PublishTestCommand { get; }
        public ICommand ViewAttemptsCommand { get; }
        public ICommand ExportExcelTestCommand { get; }

        public CuratorAllTestViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            TestService testService,
            TopicService topicService,
            CuratorService curatorService,
            ParticipantService participantService,
            GroupService groupService,
            ParticipantPublicTestService participantPublicTestService,
            IServiceProvider serviceProvider) : base(navigationService, dialogService)
        {
            _testService = testService;
            _topicService = topicService;
            _curatorService = curatorService;
            _serviceProvider = serviceProvider;
            _participantService = participantService;
            _groupService = groupService;
            _participantPublicTestService = participantPublicTestService;

            CardClickCommand = new RelayCommand(_ => OnCardClick(_ as TestListItemViewModel));
            ArchiveTestCommand = new RelayCommand(_ => _ = OnArchiveTestAsync(_ as Test));

            CreateTestCommand = new RelayCommand(_ => OnCreateTest());
            ImportExcelCommand = new RelayCommand(_ => OnImportExcel());
            CreateAddTopicsCommand = new RelayCommand(_ => _ = CreateAddTopicsAsync());
            ClearFiltersCommand = new RelayCommand(_ => _ = ClearFiltersAsync());

            PublishTestCommand = new RelayCommand(_ => _ = PublishTestAsync(_ as Test));
            AssignedTestCommand = new RelayCommand(_ => _ = AssignedTestAsync(_ as Test));
            ExportExcelTestCommand = new RelayCommand(_ => _ = ExportExcelTestAsync(_ as Test));
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadTopicsAsync();
                await LoadCuratorsAsync();
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
        private async Task PublishTestAsync(Test? test)
        {
            try
            {
                var vm = ActivatorUtilities.CreateInstance<PublicDetailsViewModel>(_serviceProvider, test);
                _dialogService.ShowWindow<ShellWindow>(vm);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
        private async Task AssignedTestAsync(Test? test)
        {
            try
            {
                var vm = ActivatorUtilities.CreateInstance<AssignedDetailsViewModel>(_serviceProvider, test);
                _dialogService.ShowWindow<ShellWindow>(vm);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
        private async Task ExportExcelTestAsync(Test? test)
        {
            try
            {
                var exportService = new TestExportService(_testService);
                exportService.ShowSaveDialogAndExport(test);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
        private async Task LoadTopicsAsync()
        {
            await _topicService.InitializeAsync();
            Topics.Clear();
            foreach (var topic in _topicService.Topics)
                Topics.Add(topic);
        }

        private async Task LoadCuratorsAsync()
        {
            await _curatorService.InitializeAsync();
            Curators.Clear();
            foreach (var curator in _curatorService.Curators.Where(c => c.Id != CurrentUser.Id))
                Curators.Add(curator);
        }

        private async Task LoadTestsAsync()
        {
            try
            {
                IsLoading = true;

                _allTests.Clear();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Tests.Clear();
                    Tests.Add(new TestListItemViewModel(true));
                    Tests.Add(new TestListItemViewModel(false));
                });

                if (CurrentUser.TypeUser == 1)
                    await _testService.GetAllAsync(null, null);
                else
                    await _testService.GetAllAsync(null, CurrentUser.Id);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var test in _testService.Tests)
                    {
                        if (test.Questions != null)
                        {
                            test.PenaltyMax = test.Questions.Count;
                        }

                        var testVM = new TestListItemViewModel(test, _testService, true);
                        Tests.Add(testVM);
                        _allTests.Add(test);
                    }
                });

                await ApplyFiltersAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки тестов: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ApplyFiltersAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var filtered = _allTests.AsEnumerable();

                    if (IsOwnAuthorship)
                        filtered = filtered.Where(t => t.CuratorCreateId == CurrentUser.Id);
                    else if (IsNotMyAuthorship && SelectedCoauthor != null)
                        filtered = filtered.Where(t => t.CuratorCreateId == SelectedCoauthor.Id);

                    if (IsSelectedArchive && !IsSelectedActive)
                        filtered = filtered.Where(t => t.IsArchive == true);
                    else if (!IsSelectedArchive && IsSelectedActive)
                        filtered = filtered.Where(t => t.IsArchive == false);

                    if (SelectedTopic != null)
                        filtered = filtered.Where(t => t.TopicId == SelectedTopic.Id);

                    if (SelectedDate.HasValue)
                        filtered = filtered.Where(t => t.DateOfCreating.Date == SelectedDate.Value.Date);

                    if (!string.IsNullOrWhiteSpace(TestNameFilter))
                        filtered = filtered.Where(t => t.Name != null &&
                            t.Name.Contains(TestNameFilter, StringComparison.OrdinalIgnoreCase));

                    Tests.Clear();
                    Tests.Add(new TestListItemViewModel(true));
                    Tests.Add(new TestListItemViewModel(false));

                    foreach (var test in filtered)
                        Tests.Add(new TestListItemViewModel(test, _testService, true));
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Ошибка фильтрации: {ex.Message}", "Ошибка");
                }
            });
        }

        public async Task RefreshTestsAsync()
        {
            await LoadTestsAsync();
        }

        private async Task ClearFiltersAsync()
        {
            IsOwnAuthorship = false;
            IsNotMyAuthorship = false;
            SelectedCoauthor = null;
            SelectedTopic = null;
            SelectedDate = null;
            TestNameFilter = "";
            SelectedCoauthorFilter = null;
            IsSelectedActive = false;
            IsSelectedArchive = false;
            await ApplyFiltersAsync();
        }

        public void OnCardClick(TestListItemViewModel? card)
        {
            if (card == null) return;

            if (card.IsCreateCard && !card.IsExcelCard)
            {
                OnCreateTest();
                return;
            }
            else if (card.IsExcelCard)
            {
                OnImportExcel();
                return;
            }

            if (card.Test != null)
            {
                IsLoading = true;
                var viewModel = ActivatorUtilities.CreateInstance<CuratorCreateTestViewModel>(
                    _serviceProvider, 
                    card.Test);

                _navigationService.NavigateTo(viewModel);
                IsLoading = false;
            }
        }

        public void OnCreateTest()
        {
            _navigationService.NavigateTo(_serviceProvider.GetRequiredService<CuratorCreateTestViewModel>());
        }

        public void OnImportExcel()
        {
            var vm = _serviceProvider.GetRequiredService<ImportExcelViewModel>();
            _dialogService.ShowWindow<ShellWindow>(vm);
        }

        public async Task CreateAddTopicsAsync()
        {
            var vm = _serviceProvider.GetRequiredService<CreateEditTopicViewModel>();
            _dialogService.ShowWindow<ShellWindow>(vm);
        }

        public async Task OnArchiveTestAsync(Test? test)
        {
            if (test == null) return;

            try
            {
                string action = test.IsArchive == true ? "восстановить из архива" : "отправить в архив";
                string title = test.IsArchive == true ? "Восстановление теста" : "Архивация теста";
                string message = test.IsArchive == true
                    ? $"Восстановить «{test.Name}» из архива?"
                    : $"Отправить «{test.Name}» в архив?";

                var confirmed = _dialogService.ShowConfirmation(message, title);
                if (!confirmed) return;

                test.IsArchive = !test.IsArchive;

                await _testService.UpdateAsync(test);
                await LoadTestsAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при изменении статуса теста: {ex.Message}", "Ошибка");
            }
        }

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
    }
}