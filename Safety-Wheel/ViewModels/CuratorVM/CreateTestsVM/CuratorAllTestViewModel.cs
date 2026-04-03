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

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class CuratorAllTestViewModel : BaseViewModel
    {
        private readonly TestService _testService = new();
        private readonly Topic _subject;
        private List<Test> _allTests = new();

        public ObservableCollection<TestListItemViewModel> Tests { get; } = new();
        public ObservableCollection<Topic> Topics { get; } = new();
        public ObservableCollection<Curator> Curators { get; } = new();

        private bool _isOwnAuthorship = true;
        public bool IsOwnAuthorship
        {
            get => _isOwnAuthorship;
            set { _isOwnAuthorship = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private bool _isMyAuthorshipWith;
        public bool IsNotMyAuthorship
        {
            get => _isMyAuthorshipWith;
            set { _isMyAuthorshipWith = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private Curator? _selectedCoauthor;
        public Curator? SelectedCoauthor
        {
            get => _selectedCoauthor;
            set { _selectedCoauthor = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private Topic? _selectedTopic;
        public Topic? SelectedTopic
        {
            get => _selectedTopic;
            set { _selectedTopic = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _testNameFilter = "";
        public string TestNameFilter
        {
            get => _testNameFilter;
            set { _testNameFilter = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private Curator? _selectedCoauthorFilter;
        public Curator? SelectedCoauthorFilter
        {
            get => _selectedCoauthorFilter;
            set { _selectedCoauthorFilter = value; OnPropertyChanged(); ApplyFilters(); }
        }

        public ICommand CardClickCommand { get; }
        public ICommand ArchiveTestCommand { get; }
        public ICommand CreateTestCommand { get; }
        public ICommand ImportExcelCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public CuratorAllTestViewModel(
            INavigationService navigationService,
            IDialogService dialogService) : base(navigationService, dialogService)
        {
            CardClickCommand = new RelayCommand(_ => OnCardClick(_ as TestListItemViewModel));
            ArchiveTestCommand = new RelayCommand(_ => OnArchiveTest(_ as Test));
            CreateTestCommand = new RelayCommand(_ => OnCreateTest());
            ImportExcelCommand = new RelayCommand(_ => OnImportExcel());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

            LoadTopics();
            LoadCurators();
            LoadTests();
        }

        private void LoadTopics()
        {
            var service = new TopicService();
            service.GetAll();
            Topics.Clear();
            foreach (var topic in service.Topics)
                Topics.Add(topic);
        }

        private void LoadCurators()
        {
            var service = new CuratorService();
            service.GetAll();
            Curators.Clear();
            foreach (var curator in service.Curators.Where(c => c.Id != CurrentUser.Id))
                Curators.Add(curator);
        }

        private void LoadTests()
        {
            _allTests.Clear();
            Tests.Clear();

            Tests.Add(new TestListItemViewModel(true));
            Tests.Add(new TestListItemViewModel(false));

            if (CurrentUser.TypeUser == 1)
                _testService.GetAll(null, null);
            else
                _testService.GetAll(null, CurrentUser.Id);

            foreach (var test in _testService.Tests)
            {
                Tests.Add(new TestListItemViewModel(test, _testService));
                _allTests.Add(test);
            }
                
            ApplyFilters();
        }

        ////мое
        //private void LoadTests()
        //{
        //    Tests.Clear();
        //    Tests.Add(new TestListItemViewModel(true));
        //    Tests.Add(new TestListItemViewModel(false));

        //    if (_subject == null)
        //        _testService.GetAll(null, CurrentUser.Id);
        //    else
        //        _testService.GetTestsByTopicId(_subject.Id, CurrentUser.Id);

        //    foreach (var test in _testService.Tests)
        //        Tests.Add(new TestListItemViewModel(test, _testService));
        //}

        private void ApplyFilters()
        {
            var filtered = _allTests.AsEnumerable();

            if (IsOwnAuthorship)
                filtered = filtered.Where(t => t.CuratorCreateId == CurrentUser.Id);
            else if (IsNotMyAuthorship && SelectedCoauthor != null)
                filtered = filtered.Where(t =>
                    (t.CuratorCreateId == SelectedCoauthor.Id));

            if (SelectedTopic != null)
                filtered = filtered.Where(t => t.TopicId == SelectedTopic.Id);

            if (SelectedDate.HasValue)
                filtered = filtered.Where(t => t.DateOfCreating.Date == SelectedDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(TestNameFilter))
                filtered = filtered.Where(t => t.Name != null &&
                    t.Name.Contains(TestNameFilter, StringComparison.OrdinalIgnoreCase));

            //if (SelectedCoauthorFilter != null)
            //    filtered = filtered.Where(t => t.CoauthorId == SelectedCoauthorFilter.Id);

            Tests.Clear();
            Tests.Add(new TestListItemViewModel(true));
            Tests.Add(new TestListItemViewModel(false));

            foreach (var test in filtered)
                Tests.Add(new TestListItemViewModel(test, _testService));
        }

        private void ClearFilters()
        {
            IsOwnAuthorship = false;
            IsNotMyAuthorship = false;
            SelectedCoauthor = null;
            SelectedTopic = null;
            SelectedDate = null;
            TestNameFilter = "";
            SelectedCoauthorFilter = null;
            ApplyFilters();
        }

        public void OnCardClick(TestListItemViewModel? item)
        {
            if (item == null) return;

            if (item.IsCreateCard && !item.IsExcelCard)
            {
                OnCreateTest();
                return;
            }
            else if (item.IsExcelCard)
            {
                OnImportExcel();
                return;
            }

            if (item.Test != null)
            {
                IsLoading = true;
                _navigationService.NavigateTo(new CuratorCreateTestViewModel(_navigationService,
                    _dialogService,
                    item.Test));
                IsLoading = false;
            }
        }

        public void OnCreateTest()
        {
            _navigationService.NavigateTo(new CuratorCreateTestViewModel(_navigationService,
                    _dialogService));
        }

        public void OnImportExcel()
        {
            var vm = new ImportExcelViewModel(_navigationService, _dialogService);
            _dialogService.ShowWindow<ShellWindow>(vm);
        }

        public void OnArchiveTest(Test? test)
        {
            if (test == null) return;

            var confirmed = _dialogService.ShowConfirmation(
                $"Отправить тест «{test.Name}» в архив?",
                "Подтверждение");

            if (!confirmed) return;

            test.IsArchive = true;
            _testService.Update(test);

            LoadTests();
            _dialogService.ShowMessage("Тест отправлен в архив", "Готово");
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