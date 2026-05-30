using CozyTest.Models;
using CozyTest.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CozyTest.ViewModels.CuratorVM.ShowPassingVM
{
    public class CuratorShowAssignedPassingCurrentTestViewModel : BaseViewModel
    {
        public override string WindowTitle => "Прохождение теста";

        private readonly IServiceProvider _serviceProvider;
        private readonly GroupService _groupService;
        private readonly ParticipantService _participantService;
        private readonly AttemptService _attemptService;
        private readonly ParticipantAssignedTestService _participantAssignedTestService;
        private readonly Test _currentTest;

        public ObservableCollection<GroupProgressItem> GroupsList { get; } = new();
        public ObservableCollection<ParticipantProgressItem> ParticipantsList { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand GoToResultsCommand { get; }

        public CuratorShowAssignedPassingCurrentTestViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            IServiceProvider serviceProvider,
            ParticipantService participantService,
            GroupService groupService,
            AttemptService attemptService,
            ParticipantAssignedTestService participantAssignedTestService,
            Test test, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            CurrentUser.AdminModeOnChanged += async (_, _) =>
            {
                OnPropertyChanged(nameof(AdminModeOn));
                await LoadDataAsync();
            };
            _serviceProvider = serviceProvider;
            _groupService = groupService;
            _participantService = participantService;
            _attemptService = attemptService;
            _participantAssignedTestService = participantAssignedTestService;
            _currentTest = test;

            GoToResultsCommand = new RelayCommand(_ => GoToResults());

            _ = LoadDataAsync();
        }

        private void GoToResults()
        {
            var win = ActivatorUtilities.CreateInstance<CuratorShowPassingTestsViewModel>(_serviceProvider, _currentTest);
            _dialogService.ShowWindow<ShellWindow>(win);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);

                var assignedParticipantIds = _participantAssignedTestService.Assignments
                    .Where(a => a.TestId == _currentTest.Id)
                    .Select(a => a.ParticipantId)
                    .Distinct()
                    .ToList();

                await _participantService.GetAllParticipantsAsync(true, CurrentUser.Id);
                await _attemptService.GetAllAsync(testId: _currentTest.Id);

                var allGroups = await _groupService.GetAllGroupsForCuratorAsync(true, CurrentUser.Id, _currentTest.Id);

                foreach (var group in allGroups)
                {
                    var participantsInGroup = await _groupService.GetAllParticipantForGroup(group.Id);

                    if (!participantsInGroup.Any()) continue;

                    var participantIdsInGroup = participantsInGroup.Select(p => p.Id).ToList();

                    bool allAssigned = participantIdsInGroup.All(id => assignedParticipantIds.Contains(id));

                    if (!allAssigned) continue;

                    var passedCount = participantsInGroup.Count(p =>
                    {
                        var participantAttempts = _attemptService.Attempts
                            .Where(a => a.TestId == _currentTest.Id && a.ParticipantId == p.Id)
                            .ToList();

                        if (!participantAttempts.Any()) return false;

                        var lastAttempt = participantAttempts
                            .OrderByDescending(a => a.FinishedAt)
                            .ThenByDescending(a => a.Id)
                            .First();

                        return lastAttempt.FinishedAt.HasValue;
                    });

                    GroupsList.Add(new GroupProgressItem
                    {
                        Id = group.Id,
                        Name = group.Name,
                        TotalParticipants = participantsInGroup.Count,
                        PassedCount = passedCount,
                        ProgressPercent = participantsInGroup.Count > 0
                            ? (double)passedCount / participantsInGroup.Count * 100
                            : 0
                    });
                }

                var assignedParticipants = _participantService.Participants
                    .Where(p => assignedParticipantIds.Contains(p.Id))
                    .ToList();

                foreach (var participant in assignedParticipants)
                {
                    var hasPassed = _attemptService.Attempts
                        .Any(a => a.ParticipantId == participant.Id && a.TestId == _currentTest.Id);

                    ParticipantsList.Add(new ParticipantProgressItem
                    {
                        Id = participant.Id,
                        Name = participant.Name,
                        Login = participant.Login,
                        IsPassed = hasPassed,
                        RowColor = hasPassed
                            ? new SolidColorBrush(Colors.LightGreen)
                            : new SolidColorBrush(Colors.LightCoral)
                    });
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class GroupProgressItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TotalParticipants { get; set; }
        public int PassedCount { get; set; }
        public double ProgressPercent { get; set; }
        public string ProgressText => $"{PassedCount} / {TotalParticipants}";
    }

    public class ParticipantProgressItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public bool IsPassed { get; set; }
        public Brush RowColor { get; set; }
        public string StatusText => IsPassed ? "Пройдено" : "Не пройдено";
    }
}