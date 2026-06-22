using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.ShowPassingVM
{
    public class CuratorShowPassingTestsViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        public override string WindowTitle => "Просмотр прохождений тестов";

        private readonly AttemptService _attemptService;
        private readonly TestService _testService;
        private readonly ParticipantService _participantService;
        private readonly TopicService _topicService;
        private readonly CuratorService _curatorService;
        private readonly CriteriaService _criteriaService;
        private readonly GroupService _groupService;

        private string _participantNameFilter = string.Empty;
        private string _testNameFilter = string.Empty;
        private Topic? _selectedTopic;
        private Curator? _selectedAuthor;
        private Group? _selectedGroup;
        private DateTime? _selectedDate;
        private string? _selectedStatus;
        private bool _isSelectedArchive;
        private bool _isShowLast;
        private bool _isSelectedActive = true;
        private bool _isCurrentTest = false;
        private Curator? _selectedCoauthor;

        private ObservableCollection<Curator> _curators = new();
        private ObservableCollection<Topic> _topics = new();
        private ObservableCollection<Group> _groups = new();
        private ObservableCollection<string> _statuses = new() { "В процессе", "Завершен" };

        private ObservableCollection<AttemptDisplayModel> _attemptsList = new();
        private AttemptDisplayModel? _selectedAttempt;


        public string ParticipantNameFilter
        {
            get => _participantNameFilter;
            set
            {
                if (SetProperty(ref _participantNameFilter, value))
                    LoadAttemptsAsync();
            }
        }

        public string TestNameFilter
        {
            get => _testNameFilter;
            set
            {
                if (SetProperty(ref _testNameFilter, value))
                    LoadAttemptsAsync();
            }
        }

        public Topic? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                if (SetProperty(ref _selectedTopic, value))
                    LoadAttemptsAsync();
            }
        }

        public Curator? SelectedAuthor
        {
            get => _selectedAuthor;
            set
            {
                if (SetProperty(ref _selectedAuthor, value))
                    LoadAttemptsAsync();
            }
        }
        public Group? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                    LoadAttemptsAsync();
            }
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                    LoadAttemptsAsync();
            }
        }

        public string? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                    LoadAttemptsAsync();
            }
        }
        public double _midleScore;
        public double MiddleScore
        {
            get => _midleScore;
            set
            {
                if (SetProperty(ref _midleScore, value)) ;
            }
        }
        private int _countQuest;
        public int CountQuest
        {
            get => _countQuest;
            set { _countQuest = value; OnPropertyChanged(); }
        }

        private double _averageScore;
        public double AverageScore
        {
            get => _averageScore;
            set { _averageScore = value; OnPropertyChanged(); }
        }
        private int _totalResponses;
        public int TotalResponses
        {
            get => _totalResponses;
            set { _totalResponses = value; OnPropertyChanged(); }
        }

        private async Task MiddleScoreTaks()
        {
            if (SelectedAttempt == null) return;

            var attempts = await _attemptService.GetAttemptsByTestAsync(SelectedAttempt.TestId);

            TotalResponses = attempts.Count;

            var scores = attempts
                .Where(a => a.Score.HasValue)
                .Select(a => a.Score.Value)
                .OrderBy(s => s)
                .ToList();

            if (scores.Any())
            {
                CountQuest = (int)attempts.FirstOrDefault(p=>p.Id == SelectedAttempt.Id).CountQuestions;
                MiddleScore = Math.Round(scores.Average(), 1);
            }
            else
            {
                MiddleScore = 0;
            }
        }
        public bool IsSelectedArchive
        {
            get => _isSelectedArchive;
            set
            {
                if (SetProperty(ref _isSelectedArchive, value))
                {
                    if (value) IsSelectedActive = false;
                    LoadAttemptsAsync();
                }
            }
        }
        public bool IsShowLast
        {
            get => _isShowLast;
            set
            {
                SetProperty(ref _isShowLast, value);
                LoadAttemptsAsync();
            }
        }

        public bool IsSelectedActive
        {
            get => _isSelectedActive;
            set
            {
                if (SetProperty(ref _isSelectedActive, value))
                {
                    if (value) IsSelectedArchive = false;
                    LoadAttemptsAsync();
                }
            }
        }
        public bool IsCurrentTest
        {
            get => _isCurrentTest;
            set => SetProperty(ref _isCurrentTest, value);
        }

        public Curator? SelectedCoauthor
        {
            get => _selectedCoauthor;
            set
            {
                if (SetProperty(ref _selectedCoauthor, value))
                    LoadAttemptsAsync();
            }
        }

        public ObservableCollection<Curator> Curators
        {
            get => _curators;
            set => SetProperty(ref _curators, value);
        }

        public ObservableCollection<Topic> Topics
        {
            get => _topics;
            set => SetProperty(ref _topics, value);
        }
        public ObservableCollection<Group> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public ObservableCollection<string> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        public ObservableCollection<AttemptDisplayModel> AttemptsList
        {
            get => _attemptsList;
            set => SetProperty(ref _attemptsList, value);
        }

        public AttemptDisplayModel? SelectedAttempt
        {
            get => _selectedAttempt;
            set
            {
                SetProperty(ref _selectedAttempt, value);

                MiddleScoreTaks();
            }
        }
        public Test? _currentTest;
        public Participant? _currentParticipant;

        public ICommand ClearFiltersCommand { get; }
        public ICommand GoCurrentAttemptCommand { get; }
        public ICommand SelectTestCommand { get; }
        public ICommand SelectParticipantCommand { get; }

        public CuratorShowPassingTestsViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            IServiceProvider serviceProvider,
            AttemptService attemptService,
            TestService testService,
            ParticipantService participantService,
            TopicService topicService,
            CriteriaService criteriaService,
            GroupService groupService,
            CuratorService curatorService, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            CurrentUser.AdminModeOnChanged += async (_, _) =>
            {
                OnPropertyChanged(nameof(AdminModeOn));
                await LoadListAttemptAsync();    
                await LoadAttemptsAsync();     
            };

            _attemptService = attemptService;
            _testService = testService;
            _participantService = participantService;
            _topicService = topicService;
            _curatorService = curatorService;
            _serviceProvider = serviceProvider;
            _criteriaService = criteriaService;
            _groupService = groupService;
           
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            GoCurrentAttemptCommand = new RelayCommand(_ => GoCurrentAttemp());

            SelectTestCommand = new RelayCommand(_ => SelectTest());
            SelectParticipantCommand = new RelayCommand(_ => SelectParticipant());

            _ = LoadInitialDataAsync();
        }
        
        public CuratorShowPassingTestsViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            IServiceProvider serviceProvider,
            AttemptService attemptService,
            TestService testService,
            ParticipantService participantService,
            TopicService topicService,
            CriteriaService criteriaService,
            CuratorService curatorService, GroupService groupService,
            Test test, ILoggingService logger) : base(navigationService, dialogService, logger)
        {

            CurrentUser.AdminModeOnChanged += async (_, _) =>
            {
                OnPropertyChanged(nameof(AdminModeOn));
                await LoadListAttemptAsync();    
                await LoadAttemptsAsync();       
            };

            IsCurrentTest = true;
            _currentTest = test;

            _attemptService = attemptService;
            _testService = testService;
            _participantService = participantService;
            _topicService = topicService;
            _curatorService = curatorService;
            _serviceProvider = serviceProvider;
            _criteriaService = criteriaService;
            _groupService = groupService;

            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            GoCurrentAttemptCommand = new RelayCommand(_ => GoCurrentAttemp());

            _ = LoadInitialDataAsync();
            IsSelectedActive = false;
        }
        public CuratorShowPassingTestsViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            IServiceProvider serviceProvider,
            AttemptService attemptService,
            TestService testService,
            ParticipantService participantService,
            TopicService topicService,
            CriteriaService criteriaService,
            CuratorService curatorService, GroupService groupService,
            Participant participant, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _currentParticipant = participant;

            _attemptService = attemptService;
            _testService = testService;
            _participantService = participantService;
            _topicService = topicService;
            _curatorService = curatorService;
            _serviceProvider = serviceProvider;
            _criteriaService = criteriaService;
            _groupService = groupService;

            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            GoCurrentAttemptCommand = new RelayCommand(_ => GoCurrentAttemp());

            _ = LoadInitialDataAsync();
            IsSelectedActive = false;
        }


        public void GoCurrentAttemp()
        {
            if (SelectedAttempt == null) return;

            var vm = ActivatorUtilities.CreateInstance<CuratorShowPassingCurrentTestViewModel>(_serviceProvider, this);
            _dialogService.ShowWindow<ShellWindow>(vm);

        }
        private async Task LoadInitialDataAsync()
        {
            try
            {

                await _curatorService.GetAllAsync();
                await _topicService.GetAllAsync();
                await _criteriaService.GetAllAsync();
                await _groupService.GetAllGroupsForUserAsync();

                await LoadListAttemptAsync();

                IsLoading = true;

                var curators = _curatorService.Curators;
                var topics = _topicService.Topics;
                var group = _groupService.Groups;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Curators = new ObservableCollection<Curator>(curators);
                    Topics = new ObservableCollection<Topic>(topics);
                    Groups = new ObservableCollection<Group>(group);
                });

                await LoadAttemptsAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка загрузки данных: {ex.Message}", "Ошибка"));
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async Task LoadListAttemptAsync()
        {
            if (CurrentUser.TypeUser == 1 && AdminModeOn == true)
            {
                if (_currentTest == null)
                    await _attemptService.GetAllAsync();
                else
                    await _attemptService.GetAllAsync(testId: _currentTest.Id);
            }
            else if (CurrentUser.TypeUser == 1 )
            {
                if (_currentTest == null)
                    await _attemptService.GetAllAsync(curatorId: CurrentUser.Id);
                else
                    await _attemptService.GetAllAsync(testId: _currentTest.Id, curatorId: CurrentUser.Id);
            }
            else if (_currentParticipant != null)
            {
                await _attemptService.GetAllAsync(participantId: CurrentUser.Id);
            }
        }

        private async Task SelectTest()
        {
            TestNameFilter = SelectedAttempt.TestName;
        }

        private async Task SelectParticipant()
        {
            ParticipantNameFilter = SelectedAttempt.ParticipantName;
        }

        private async Task LoadAttemptsAsync()
        {
            try
            {
                IsLoading = true;

                var allAttempts = _attemptService.Attempts;
                var filtered = allAttempts.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(ParticipantNameFilter))
                {
                    var filter = ParticipantNameFilter.ToLower();
                    filtered = filtered.Where(a => a.Participant?.Name?.ToLower().Contains(filter) == true);
                }

                if (!string.IsNullOrWhiteSpace(TestNameFilter))
                {
                    var filter = TestNameFilter.ToLower();
                    filtered = filtered.Where(a => a.Test?.Name?.ToLower().Contains(filter) == true);
                }

                if (SelectedTopic != null)
                {
                    filtered = filtered.Where(a => a.Test?.TopicId == SelectedTopic.Id);
                }

                if (SelectedAuthor != null)
                {
                    filtered = filtered.Where(a => a.Test?.CuratorCreateId == SelectedAuthor.Id);
                }

                if (SelectedGroup != null)
                {
                    var participantsInGroup = await _participantService
                        .GetAllParticipantForGroupAsync(SelectedGroup.Id);

                    var participantIdsInGroup = participantsInGroup
                        .Select(p => p.Id)
                        .ToList();

                    filtered = filtered.Where(a => participantIdsInGroup.Contains((int)a.ParticipantId));
                }

                if (SelectedDate.HasValue)
                {
                    var date = SelectedDate.Value.Date;
                    filtered = filtered.Where(a => a.StartedAt.HasValue && a.StartedAt.Value.Date == date);
                }

                if (!string.IsNullOrWhiteSpace(SelectedStatus))
                {
                    filtered = filtered.Where(a => a.Status == SelectedStatus);
                }

                if (IsShowLast == true)
                {
                    filtered = filtered
                        .GroupBy(a => new { a.ParticipantId, a.TestId })
                        .Select(g => g.OrderByDescending(a => a.AttemptNumber).First());
                }

                if (IsSelectedArchive && !IsSelectedActive)
                {
                    filtered = filtered.Where(a => a.Test?.IsArchive == true);
                }
                else if (IsSelectedActive && !IsSelectedArchive)
                {
                    filtered = filtered.Where(a => a.Test?.IsArchive != true);
                }

               
                if (SelectedCoauthor != null)
                {
                    filtered = filtered.Where(a => a.Test?.Curators.Any(c => c.Id == SelectedCoauthor.Id) == true);
                }

                var displayList = new List<AttemptDisplayModel>();

                foreach (var a in filtered)
                {
                    string ml = _criteriaService.Criteria.Where(c => c.TestId == a.TestId && c.OrderNumber == a.MarkLvl).Select(c => c.Name).FirstOrDefault();
                    displayList.Add(new AttemptDisplayModel
                    {
                        Id = a.Id,
                        TestId = (int)a.TestId,
                        ParticipantName = a.Participant?.Name ?? "Неизвестно",
                        TestName = a.Test?.Name ?? "Неизвестно",
                        TopicName = a.Test?.Topic?.Name ?? "-",
                        StartedAt = a.StartedAt,
                        FinishedAt = a.FinishedAt,
                        Score = a.Score,
                        MaxScore = a.CountQuestions ?? 0,
                        MarkLvlName = ml,
                        MarkLvl = a.MarkLvl,
                        Status = a.Status ?? "-",
                        AttemptNumber = a.AttemptNumber ?? 1,
                        TestAuthor = a.Test?.CuratorCreate?.Name ?? "-",
                        IsTestArchive = a.Test?.IsArchive == true
                    });
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AttemptsList = new ObservableCollection<AttemptDisplayModel>(displayList);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка загрузки прохождений: {ex.Message}", "Ошибка"));
            }
            finally
            {
                IsLoading = false;
            }
        }


        private void ClearFilters()
        {
            ParticipantNameFilter = string.Empty;
            TestNameFilter = string.Empty;
            SelectedTopic = null;
            SelectedAuthor = null;
            SelectedDate = null;
            SelectedStatus = null;
            IsSelectedArchive = false;
            if (_currentTest == null) IsSelectedActive = true;
            SelectedCoauthor = null;
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
    }

    public class AttemptDisplayModel : ObservableObject
    {
        private int _id;
        private int _testId;
        private string _participantName = string.Empty;
        private string _testName = string.Empty;
        private string _topicName = string.Empty;
        private DateTime? _startedAt;
        private DateTime? _finishedAt;
        private int? _score;
        private string? _markLvlName;
        private int? _markLvl;
        private int _maxScore;
        private string _status = string.Empty;
        private int _attemptNumber;
        private string _testAuthor = string.Empty;
        private bool _isTestArchive;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        
        public int TestId
        {
            get => _testId;
            set => SetProperty(ref _testId, value);
        }

        public string ParticipantName
        {
            get => _participantName;
            set => SetProperty(ref _participantName, value);
        }

        public string TestName
        {
            get => _testName;
            set => SetProperty(ref _testName, value);
        }

        public string TopicName
        {
            get => _topicName;
            set => SetProperty(ref _topicName, value);
        }

        public DateTime? StartedAt
        {
            get => _startedAt;
            set => SetProperty(ref _startedAt, value);
        }

        public DateTime? FinishedAt
        {
            get => _finishedAt;
            set => SetProperty(ref _finishedAt, value);
        }

        public int? Score
        {
            get => _score;
            set => SetProperty(ref _score, value);
        }
        public string? MarkLvlName
        {
            get => _markLvlName;
            set => SetProperty(ref _markLvlName, value);
        }
        public int? MarkLvl
        {
            get => _markLvl;
            set => SetProperty(ref _markLvl, value);
        }

        public int MaxScore
        {
            get => _maxScore;
            set => SetProperty(ref _maxScore, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public int AttemptNumber
        {
            get => _attemptNumber;
            set => SetProperty(ref _attemptNumber, value);
        }

        public string TestAuthor
        {
            get => _testAuthor;
            set => SetProperty(ref _testAuthor, value);
        }

        public bool IsTestArchive
        {
            get => _isTestArchive;
            set => SetProperty(ref _isTestArchive, value);
        }

        public string ScoreDisplay => Score.HasValue ? $"{Score} / {MaxScore}" : $"- / {MaxScore}";

        public string Duration
        {
            get
            {
                if (!StartedAt.HasValue || !FinishedAt.HasValue)
                    return "-";
                var duration = FinishedAt.Value - StartedAt.Value;
                return $"{duration.Minutes:D2}:{duration.Seconds:D2}";
            }
        }
    }
}