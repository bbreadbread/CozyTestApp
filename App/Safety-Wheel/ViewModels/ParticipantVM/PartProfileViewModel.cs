using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.ParticipantVM
{
    public class PartProfileViewModel : BaseViewModel
    {
        public override string WindowTitle => "Профиль тестируемого";

        private readonly ParticipantService _participantService;
        private readonly AttemptService _attemptService;
        private readonly ParticipantAssignedTestService _assignedTestService;
        private readonly GroupService _groupService;
        private readonly CuratorService _curatorService;
        private readonly TestService _testService;

        private Participant _currentParticipant;
        private DateTime? _selectedDate;
        private ObservableCollection<ParticipantsAssignedTest> _assignmentsOnSelectedDate;
        private int _totalTestsPassed;
        private int _totalQuizzesPassed;
        private string _testWithMostAttempts;
        private ObservableCollection<Curator> _curatorsList;
        private ObservableCollection<Group> _groupsList;
        

        private string _creatorNameText;
        private string _accLoginText;
        private string _accNameText;
        private ParticipantsAssignedTest _selectedAssignedTest;

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                LoadAssignmentsForSelectedDate();
            }
        }

        public ObservableCollection<ParticipantsAssignedTest> AssignmentsOnSelectedDate
        {
            get => _assignmentsOnSelectedDate;
            set
            {
                _assignmentsOnSelectedDate = value;
                OnPropertyChanged();
            }
        }

        public ParticipantsAssignedTest SelectedAssignedTest
        {
            get => _selectedAssignedTest;
            set
            {
                _selectedAssignedTest = value;
                OnPropertyChanged();
            }
        }

        public int TotalTestsPassed
        {
            get => _totalTestsPassed;
            set
            {
                _totalTestsPassed = value;
                OnPropertyChanged();
            }
        }

        public int TotalQuizzesPassed
        {
            get => _totalQuizzesPassed;
            set
            {
                _totalQuizzesPassed = value;
                OnPropertyChanged();
            }
        }

        public string TestWithMostAttempts
        {
            get => _testWithMostAttempts;
            set
            {
                _testWithMostAttempts = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Curator> CuratorsList
        {
            get => _curatorsList;
            set
            {
                _curatorsList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Group> GroupsList
        {
            get => _groupsList;
            set
            {
                _groupsList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DateTime> _datesWithAssignments;
        public ObservableCollection<DateTime> DatesWithAssignments
        {
            get => _datesWithAssignments;
            set
            {
                _datesWithAssignments = value;
                OnPropertyChanged();
            }
        }

        public string CreatorNameText
        {
            get => _creatorNameText;
            set
            {
                _creatorNameText = value;
                OnPropertyChanged();
            }
        }
        public string AccLoginText
        {
            get => _accLoginText;
            set
            {
                _accLoginText = value;
                OnPropertyChanged();
            }
        }
        public string AccNameText
        {
            get => _accNameText;
            set
            {
                _accNameText = value;
                OnPropertyChanged();
            }
        }

        public ICommand GoToAssignedTestsCommand { get; }
        public ICommand GoToAllAttemptsCommand { get; }

        public PartProfileViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            ParticipantService participantService,
            AttemptService attemptService,
            ParticipantAssignedTestService assignedTestService,
            GroupService groupService,
            CuratorService curatorService,
            TestService testService) : base(navigationService, dialogService)
        {
            _participantService = participantService;
            _attemptService = attemptService;
            _assignedTestService = assignedTestService;
            _groupService = groupService;
            _curatorService = curatorService;
            _testService = testService;

            GoToAssignedTestsCommand = new RelayCommand(_ => GoToAssignedTests());
            GoToAllAttemptsCommand = new RelayCommand(_ => GoToAllAttempts());

            _datesWithAssignments = new ObservableCollection<DateTime>();
            DatesWithAssignments = _datesWithAssignments;

            SelectedDate = DateTime.Today;

            Task.Run(async () => await LoadDataAsync());
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                _currentParticipant = await _participantService.GetByIdAsync(CurrentUser.Id);

                await LoadAttemptsStatistics();
                await LoadCuratorsAndGroups();

                await _assignedTestService.GetParticipantAssignmentsAsync(CurrentUser.Id);

                LoadDatesWithAssignments();

                LoadAssignmentsForSelectedDate();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AccNameText = $"{(_currentParticipant?.Name ?? "Неизвестно")}";
                    AccLoginText = $"Логин: {(_currentParticipant?.Login ?? "Неизвестно")}";
                    CreatorNameText = $"Создатель аккаунта: {(_currentParticipant?.CuratorCreate?.Name ?? "Неизвестен")}";
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка загрузки профиля: {ex.Message}", "Ошибка"));
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = false);
            }
        }

        private async Task LoadAttemptsStatistics()
        {
            var attempts = await _attemptService.GetParticipantAttemptsAsync(CurrentUser.Id);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var tests = attempts.Where(a => a.Test?.TestType?.Name == "Тест").ToList();
                var quizzes = attempts.Where(a => a.Test?.TestType?.Name == "Опросник").ToList();

                TotalTestsPassed = tests.Count;
                TotalQuizzesPassed = quizzes.Count;

                var testAttemptsGroup = attempts
                    .Where(a => a.Test != null)
                    .GroupBy(a => a.Test.Name)
                    .Select(g => new { TestName = g.Key, AttemptCount = g.Count() })
                    .OrderByDescending(g => g.AttemptCount)
                    .FirstOrDefault();

                if (testAttemptsGroup != null)
                {
                    TestWithMostAttempts = $"{testAttemptsGroup.TestName} ({testAttemptsGroup.AttemptCount} попыток прохождения)";
                }
                else
                {
                    TestWithMostAttempts = "Нет данных";
                }
            });
        }

        private async Task LoadCuratorsAndGroups()
        {
            var curators = await _curatorService.GetCuratorsForParticipantAsync(CurrentUser.Id);
            await _groupService.GetAllGroupsForUserAsync(CurrentUser.Id);
            var groups = _groupService.Groups;

            foreach (var group in groups)
            {
                {
                    var o = await _groupService.GetAllParticipantForGroup(group.Id);
                    group.CountPart = o.Count();
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CuratorsList = new ObservableCollection<Curator>(curators);
                    GroupsList = new ObservableCollection<Group>(groups);
                });
            }
        }

        private void LoadDatesWithAssignments()
        {
            var dates = _assignedTestService.Assignments
                .Where(a => a.DateTimeAssigned.HasValue)
                .Select(a => a.DateTimeAssigned.Value.Date)
                .Distinct()
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Найдено дат с назначениями: {dates.Count}");
            foreach (var date in dates)
            {
                System.Diagnostics.Debug.WriteLine($"  - {date:yyyy-MM-dd}");
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                DatesWithAssignments.Clear();
                foreach (var date in dates)
                {
                    DatesWithAssignments.Add(date);
                }

                OnPropertyChanged(nameof(DatesWithAssignments));

                if (SelectedDate.HasValue)
                {
                    LoadAssignmentsForSelectedDate();
                }
            });
        }

        private async void LoadAssignmentsForSelectedDate()
        {
            if (!SelectedDate.HasValue) return;

            try
            {
                var assignments = _assignedTestService.Assignments
                    .Where(a => a.DateTimeAssigned.HasValue &&
                               a.DateTimeAssigned.Value.Date == SelectedDate.Value.Date)
                    .ToList();

                foreach (var assignment in assignments)
                {
                    if (assignment.Test == null && assignment.TestId > 0)
                    {
                        assignment.Test = await _testService.GetTestById(assignment.TestId);
                    }
                    if (assignment.Participant == null && assignment.ParticipantId > 0)
                    {
                        assignment.Participant = await _participantService.GetByIdAsync(assignment.ParticipantId);
                    }
                    if (assignment.Curator == null && assignment.ResponsibleId > 0)
                    {
                        assignment.Curator = await _curatorService.GetById(assignment.ResponsibleId);
                    }
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AssignmentsOnSelectedDate = new ObservableCollection<ParticipantsAssignedTest>(assignments);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка загрузки назначений: {ex.Message}", "Ошибка"));
            }
        }

        public bool HasAssignmentsOnDate(DateTime date)
        {
            return DatesWithAssignments.Contains(date.Date);
        }

        private void GoToAssignedTests()
        {
            //_navigationService.NavigateTo("AssignedTestsPage");
        }

        private void GoToAllAttempts()
        {
            //_navigationService.NavigateTo("AllAttemptsPage");
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }
    }
}