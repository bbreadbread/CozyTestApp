using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class AssignedDetailsViewModel : BaseViewModel
    {
        public override string WindowTitle => "Настройка назначения теста";

        public int currentTestId = 0;
        private GroupService _groupService;
        private ParticipantService _participantService;
        private ParticipantAssignedTestService _participantAssignedTestService;
        private Test _currentTest;

        private ObservableCollection<Group> _groupsList;
        public ObservableCollection<Group> GroupsList
        {
            get => _groupsList;
            set
            {
                _groupsList = value;
                OnPropertyChanged(nameof(GroupsList));
            }
        }

        private ObservableCollection<Participant> _participantsList;
        public ObservableCollection<Participant> ParticipantsList
        {
            get => _participantsList;
            set
            {
                _participantsList = value;
                OnPropertyChanged(nameof(ParticipantsList));
            }
        }

        private ObservableCollection<ParticipantsAssignedTest> _testsOnSelectedDate;
        public ObservableCollection<ParticipantsAssignedTest> TestsOnSelectedDate
        {
            get => _testsOnSelectedDate;
            set
            {
                _testsOnSelectedDate = value;
                OnPropertyChanged(nameof(TestsOnSelectedDate));
            }
        }

        public RelayCommand AssignTestCommand { get; }
        public RelayCommand EditAssignmentCommand { get; }
        public RelayCommand DeleteAssignmentCommand { get; }
        public RelayCommand AssignToAllCommand { get; }
        public RelayCommand SwitchAssignedGroupsCommand { get; }
        public RelayCommand SwitchAssignedParticipantCommand { get; }
        public RelayCommand SaveAssignmentsCommand { get; }
        public RelayCommand CancelCommand { get; }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                OnTabSelected();
            }
        }

        private int _assignmentTabIndex;
        public int AssignmentTabIndex
        {
            get => _assignmentTabIndex;
            set
            {
                _assignmentTabIndex = value;
                OnPropertyChanged();
            }
        }

        private ParticipantsAssignedTest _selectedAssignedTest;
        public ParticipantsAssignedTest SelectedAssignedTest
        {
            get => _selectedAssignedTest;
            set
            {
                _selectedAssignedTest = value;
                OnPropertyChanged();
                CanEdit = value != null;
                CanDelete = value != null;
            }
        }

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set => SetProperty(ref _selectedParticipant, value);
        }

        private Group _selectedGroup;
        public Group SelectedGroup
        {
            get => _selectedGroup;
            set => SetProperty(ref _selectedGroup, value);
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                LoadTestsForSelectedDate();
            }
        }

        private DateTime? _assignmentDate;
        public DateTime? AssignmentDate
        {
            get => _assignmentDate;
            set
            {
                _assignmentDate = value;
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

        private bool _isAssignToAll;
        public bool IsAssignToAll
        {
            get => _isAssignToAll;
            set
            {
                _isAssignToAll = value;
                OnPropertyChanged();
                if (value)
                {
                    AssignToAllParticipants();
                }
            }
        }

        private bool _canAssign;
        public bool CanAssign
        {
            get => _canAssign;
            set
            {
                _canAssign = value;
                OnPropertyChanged();
            }
        }

        private bool _canEdit;
        public bool CanEdit
        {
            get => _canEdit;
            set
            {
                _canEdit = value;
                OnPropertyChanged();
            }
        }

        private bool _canDelete;
        public bool CanDelete
        {
            get => _canDelete;
            set
            {
                _canDelete = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public AssignedDetailsViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ParticipantService participantService,
            GroupService groupService,
            ParticipantAssignedTestService participantAssignedTestService) : base(navigationService, dialogService)
        {
            _groupService = groupService;
            _participantService = participantService;
            _participantAssignedTestService = participantAssignedTestService;
            _currentTest = null;

            AssignTestCommand = new RelayCommand(_ => OpenAssignmentTab());
            EditAssignmentCommand = new RelayCommand(_ => EditAssignment());
            DeleteAssignmentCommand = new RelayCommand(_ => DeleteAssignment());
            AssignToAllCommand = new RelayCommand(_ => AssignToAllParticipants());
            SwitchAssignedGroupsCommand = new RelayCommand(_ => SwitchAssignedGroups());
            SwitchAssignedParticipantCommand = new RelayCommand(_ => SwitchAssignedParticipant());
            SaveAssignmentsCommand = new RelayCommand(_ => SaveAssignments());
            CancelCommand = new RelayCommand(_ => Cancel());

            SelectedDate = DateTime.Today;
            AssignmentDate = DateTime.Today;
            CanAssign = true;
            CanEdit = false;
            CanDelete = false;

            Task.Run(async () => await LoadDataAsync());
        }

        public AssignedDetailsViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ParticipantService participantService,
            GroupService groupService,
            ParticipantAssignedTestService participantAssignedTestService,
            Test test) : base(navigationService, dialogService)
        {
            _groupService = groupService;
            _participantService = participantService;
            _participantAssignedTestService = participantAssignedTestService;
            _currentTest = test;
            currentTestId = test?.Id ?? 0;

            AssignTestCommand = new RelayCommand(_ => OpenAssignmentTab());
            EditAssignmentCommand = new RelayCommand(_ => EditAssignment());
            DeleteAssignmentCommand = new RelayCommand(_ => DeleteAssignment());
            AssignToAllCommand = new RelayCommand(_ => AssignToAllParticipants());
            SwitchAssignedGroupsCommand = new RelayCommand(_ => SwitchAssignedGroups());
            SwitchAssignedParticipantCommand = new RelayCommand(_ => SwitchAssignedParticipant());
            SaveAssignmentsCommand = new RelayCommand(_ => SaveAssignments());
            CancelCommand = new RelayCommand(_ => Cancel());

            SelectedDate = DateTime.Today;
            AssignmentDate = DateTime.Today;

            _datesWithAssignments = new ObservableCollection<DateTime>();
            DatesWithAssignments = _datesWithAssignments;

            CanAssign = true;
            CanEdit = false;
            CanDelete = false;

            Task.Run(async () => await LoadDataAsync());
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                await _participantService.GetAllParticipantsAsync(CurrentUser.Id);
                await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);

                LoadDatesWithAssignments();

                var groups = await _groupService.GetAllGroupsForCuratorAsync(CurrentUser.Id, currentTestId);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GroupsList = new ObservableCollection<Group>(groups);
                    ParticipantsList = _participantService.Participants;

                    UpdateGroupAssignmentStatus();
                    LoadTestsForSelectedDate();
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка загрузки: {ex.Message}", "Ошибка"));
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = false);
            }
        }

        private void OnTabSelected()
        {
            if (SelectedTabIndex == 0)
            {
                LoadTestsForSelectedDate();
            }
            else if (SelectedTabIndex == 1)
            {
                UpdateGroupAssignmentStatus();
            }
        }

        private async void LoadTestsForSelectedDate()
        {
            if (!SelectedDate.HasValue) return;

            var assignments = _participantAssignedTestService.Assignments
                .Where(a => a.DateTimeAssigned.HasValue && a.DateTimeAssigned.Value.Date == SelectedDate.Value.Date)
                .ToList();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                TestsOnSelectedDate = new ObservableCollection<ParticipantsAssignedTest>(assignments);
            });
        }

        private async Task UpdateGroupAssignmentStatus()
        {
            if (GroupsList == null) return;

            foreach (var group in GroupsList)
            {
                var participantsInGroup = await _groupService.GetAllParticipantForGroup(group.Id);
                bool allAssigned = participantsInGroup.Any() && participantsInGroup.All(p =>
                    _participantAssignedTestService.IsAssigned(currentTestId, p.Id));

                group.IsAssigned = allAssigned;
            }
        }

        private bool IsParticipantAssigned(int participantId)
        {
            return _participantAssignedTestService.IsAssigned(currentTestId, participantId);
        }

        private void OpenAssignmentTab()
        {
            SelectedTabIndex = 1;
            AssignmentDate = SelectedDate;
            IsAssignToAll = false;
            UpdateGroupAssignmentStatus();
        }

        private void EditAssignment()
        {
            if (SelectedAssignedTest == null) return;

            SelectedTabIndex = 1;
            AssignmentDate = SelectedAssignedTest.DateTimeAssigned;
        }

        private async void DeleteAssignment()
        {
            if (SelectedAssignedTest == null) return;

            var result = MessageBox.Show("Удалить назначение?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                await _participantAssignedTestService.RemoveAssignmentAsync(
                    SelectedAssignedTest.ParticipantId,
                    SelectedAssignedTest.TestId);

                await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);
                LoadDatesWithAssignments();

                LoadTestsForSelectedDate();
                await UpdateGroupAssignmentStatus();

                MessageBox.Show("Назначение удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка удаления: {ex.Message}", "Ошибка"));
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = false);
            }
        }

        private async void AssignToAllParticipants()
        {
            var allParticipants = await _participantService.GetAllAsync(CurrentUser.Id);
            if (!allParticipants.Any())
            {
                MessageBox.Show("Нет привязанных к Вам тестируемых", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

                IsAssignToAll = false;
                return;
            }

            if (!AssignmentDate.HasValue)
            {
                MessageBox.Show("Выберите дату назначения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsAssignToAll = false;
                return;
            }

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                bool allAssigned = allParticipants.All(p =>
                    _participantAssignedTestService.IsAssigned(currentTestId, p.Id));

                if (allAssigned)
                {
                    foreach (var participant in allParticipants)
                    {
                        await _participantAssignedTestService.RemoveAssignmentAsync(participant.Id, currentTestId);
                    }
                }
                else
                {
                    await _participantAssignedTestService.AssignTestToParticipantsAsync(
                        currentTestId, allParticipants, AssignmentDate.Value);
                }

                await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);
                await UpdateGroupAssignmentStatus();

                IsAssignToAll = allParticipants.All(p =>
                    _participantAssignedTestService.IsAssigned(currentTestId, p.Id));
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка"));
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = false);
            }
        }

        private async void SwitchAssignedGroups()
        {
            if (SelectedGroup == null) return;

            var participantsInGroup = await _groupService.GetAllParticipantForGroup(SelectedGroup.Id);

            if (!participantsInGroup.Any())
            {
                MessageBox.Show("В группе нет участников", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!AssignmentDate.HasValue)
            {
                MessageBox.Show("Выберите дату назначения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                bool currentStatus = participantsInGroup.All(p =>
                    _participantAssignedTestService.IsAssigned(currentTestId, p.Id));

                if (currentStatus)
                {
                    foreach (var participant in participantsInGroup)
                    {
                        await _participantAssignedTestService.RemoveAssignmentAsync(participant.Id, currentTestId);
                    }
                    SelectedGroup.IsAssigned = false;
                }
                else
                {
                    await _participantAssignedTestService.AssignTestToParticipantsAsync(
                        currentTestId, participantsInGroup, AssignmentDate.Value);
                    SelectedGroup.IsAssigned = true;
                }

                await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка"));
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = false);
            }
        }

        private async void SwitchAssignedParticipant()
        {
            if (SelectedParticipant == null) return;

            if (!AssignmentDate.HasValue)
            {
                MessageBox.Show("Выберите дату назначения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                bool isAssigned = _participantAssignedTestService.IsAssigned(currentTestId, SelectedParticipant.Id);

                if (isAssigned)
                {
                    await _participantAssignedTestService.RemoveAssignmentAsync(SelectedParticipant.Id, currentTestId);
                }
                else
                {
                    await _participantAssignedTestService.AssignTestToParticipantAsync(
                        SelectedParticipant.Id, currentTestId, AssignmentDate);
                }

                await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);
                await UpdateGroupAssignmentStatus();
                LoadTestsForSelectedDate();
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка"));
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = false);
            }
        }

        private async void SaveAssignments()
        {
            try
            {
                await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);
                LoadTestsForSelectedDate();
                SelectedTabIndex = 0;

                MessageBox.Show("Назначения сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    _dialogService.ShowMessage($"Ошибка сохранения: {ex.Message}", "Ошибка"));
            }
        }
        private void LoadDatesWithAssignments()
        {
            var dates = _participantAssignedTestService.Assignments
                .Where(a => a.DateTimeAssigned.HasValue)
                .Select(a => a.DateTimeAssigned.Value.Date)
                .Distinct()
                .ToList();

            System.Diagnostics.Debug.WriteLine($"AssignedDetails: Найдено дат с назначениями: {dates.Count}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (DatesWithAssignments == null)
                {
                    DatesWithAssignments = new ObservableCollection<DateTime>();
                }

                DatesWithAssignments.Clear();
                foreach (var date in dates)
                {
                    DatesWithAssignments.Add(date);
                }

                OnPropertyChanged(nameof(DatesWithAssignments));
            });
        }
        private void Cancel()
        {
            SelectedTabIndex = 0;
            AssignmentDate = DateTime.Today;
            IsAssignToAll = false;
            UpdateGroupAssignmentStatus();
        }
    }
}