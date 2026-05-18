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

        private ObservableCollection<Participant> _allParticipantsList;
        public ObservableCollection<Participant> AllParticipantsList
        {
            get => _allParticipantsList;
            set
            {
                _allParticipantsList = value;
                OnPropertyChanged(nameof(AllParticipantsList));
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

        public RelayCommand AllBindCommand { get; }
        public RelayCommand GroupsBindCommand { get; }
        public RelayCommand ParticipantsBindCommand { get; }

        public RelayCommand SwitchAssignedGroupsCommand { get; }
        public RelayCommand SwitchAssignedParticipantCommand { get; }
        public RelayCommand SwitchAllParticipantCommand { get; }

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
            }
        }

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set => SetProperty(ref _selectedParticipant, value);
        }

        private Participant _selectedAllParticipant;
        public Participant SelectedAllParticipant
        {
            get => _selectedAllParticipant;
            set => SetProperty(ref _selectedAllParticipant, value);
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

        private bool _isAllBindSelected;
        public bool IsAllBindSelected
        {
            get => _isAllBindSelected;
            set { _isAllBindSelected = value; OnPropertyChanged(); }
        }

        private bool _isGroupsBindSelected = true;
        public bool IsGroupsBindSelected
        {
            get => _isGroupsBindSelected;
            set { _isGroupsBindSelected = value; OnPropertyChanged(); }
        }

        private bool _isParticipantsBindSelected;
        public bool IsParticipantsBindSelected
        {
            get => _isParticipantsBindSelected;
            set { _isParticipantsBindSelected = value; OnPropertyChanged(); }
        }

        private bool _isAssignToAll;
        public bool IsAssignToAll
        {
            get => _isAssignToAll;
            set
            {
                _isAssignToAll = value;
                SetProperty(ref _isAssignToAll, value);
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
            CurrentUser.AdminModeOnChanged += async (_, _) =>
            {
                OnPropertyChanged(nameof(AdminModeOn));
                await RefreshDataAsync();
            };

            _groupService = groupService;
            _participantService = participantService;
            _participantAssignedTestService = participantAssignedTestService;
            _currentTest = null;

            AllBindCommand = new RelayCommand(_ => AllBind());
            GroupsBindCommand = new RelayCommand(_ => GroupsBind());
            ParticipantsBindCommand = new RelayCommand(_ => ParticipantsBind());

            SwitchAssignedGroupsCommand = new RelayCommand(_ => SwitchAssignedGroups());
            SwitchAssignedParticipantCommand = new RelayCommand(_ => SwitchAssignedParticipant());
            SwitchAllParticipantCommand = new RelayCommand(_ => SwitchAllParticipant());

            SelectedDate = DateTime.Today;
            AssignmentDate = DateTime.Today;

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

            AllBindCommand = new RelayCommand(_ => AllBind());
            GroupsBindCommand = new RelayCommand(_ => GroupsBind());
            ParticipantsBindCommand = new RelayCommand(_ => ParticipantsBind());

            SwitchAssignedGroupsCommand = new RelayCommand(_ => SwitchAssignedGroups());
            SwitchAssignedParticipantCommand = new RelayCommand(_ => SwitchAssignedParticipant());
            SwitchAllParticipantCommand = new RelayCommand(_ => SwitchAllParticipant());

            SelectedDate = DateTime.Today;
            AssignmentDate = DateTime.Today;

            Task.Run(async () => await LoadDataAsync());
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                await _participantService.GetAllParticipantsAsync(AdminModeOn, CurrentUser.Id);
                await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);

                LoadDatesWithAssignments();

                var groups = await _groupService.GetAllGroupsForCuratorAsync(AdminModeOn, CurrentUser.Id, currentTestId);
                var allParticipants = await _participantService.GetAllAsync(CurrentUser.Id);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GroupsList = new ObservableCollection<Group>(groups);
                    ParticipantsList = _participantService.Participants;
                    AllParticipantsList = new ObservableCollection<Participant>(allParticipants);

                    UpdateGroupAssignmentStatus();
                    UpdateParticipantsAssignedStatus();
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
                UpdateParticipantsAssignedStatus();
            }
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

        private void UpdateParticipantsAssignedStatus()
        {
            if (ParticipantsList == null) return;

            foreach (var participant in ParticipantsList)
            {
                participant.IsAssigned = _participantAssignedTestService.IsAssigned(currentTestId, participant.Id);
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
                UpdateParticipantsAssignedStatus();
                LoadDatesWithAssignments();
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
                LoadDatesWithAssignments();
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

        public async void AllBind()
        {
            var allParticipants = await _participantService.GetAllAsync(CurrentUser.Id);

            if (!allParticipants.Any())
            {
                MessageBox.Show("Нет привязанных к Вам тестируемых");
                return;
            }

            if (!AssignmentDate.HasValue)
            {
                MessageBox.Show("Выберите дату назначения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsAssignToAll == false)
            {
                await _participantAssignedTestService.AssignTestToParticipantsAsync(currentTestId, allParticipants, AssignmentDate.Value);
            }
            else
            {
                foreach (var participant in allParticipants)
                {
                    await _participantAssignedTestService.RemoveAssignmentAsync(participant.Id, currentTestId);
                }
            }

            IsAssignToAll = !IsAssignToAll;
            await RefreshDataAsync();
        }

        public void GroupsBind()
        {
            SelectedTabIndex = 1;
            AssignmentTabIndex = 0;
            IsAssignToAll = false;
        }

        public void ParticipantsBind()
        {
            SelectedTabIndex = 1;
            AssignmentTabIndex = 1;
            IsAssignToAll = false;
        }

        public async void SwitchAllParticipant()
        {
            if (SelectedAllParticipant == null) return;

            if (!AssignmentDate.HasValue)
            {
                MessageBox.Show("Выберите дату назначения", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                bool isAssigned = _participantAssignedTestService.IsAssigned(currentTestId, SelectedAllParticipant.Id);

                if (isAssigned)
                {
                    await _participantAssignedTestService.RemoveAssignmentAsync(SelectedAllParticipant.Id, currentTestId);
                }
                else
                {
                    await _participantAssignedTestService.AssignTestToParticipantAsync(
                        SelectedAllParticipant.Id, currentTestId, AssignmentDate);
                }

                await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);
                await UpdateGroupAssignmentStatus();
                LoadDatesWithAssignments();
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

        private void LoadTestsForSelectedDate()
        {
            if (!SelectedDate.HasValue) return;

            var assignments = _participantAssignedTestService.Assignments
                .Where(a => a.DateTimeAssigned.HasValue && a.DateTimeAssigned.Value.Date == SelectedDate.Value.Date)
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                TestsOnSelectedDate = new ObservableCollection<ParticipantsAssignedTest>(assignments);
            });
        }

        private async Task RefreshDataAsync()
        {
            await _participantService.GetAllParticipantsAsync(AdminModeOn, CurrentUser.Id);
            await _participantAssignedTestService.GetAllAssignmentsForCuratorAsync(CurrentUser.Id);

            LoadDatesWithAssignments();

            var groups = await _groupService.GetAllGroupsForCuratorAsync(AdminModeOn, CurrentUser.Id, currentTestId);
            var allParticipants = await _participantService.GetAllAsync(CurrentUser.Id);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                GroupsList = new ObservableCollection<Group>(groups);
                ParticipantsList = _participantService.Participants;
                AllParticipantsList = new ObservableCollection<Participant>(allParticipants);

                UpdateGroupAssignmentStatus();
                UpdateParticipantsAssignedStatus();
                LoadTestsForSelectedDate();

                CheckIfAllParticipantsSelected();
            });
        }

        private void CheckIfAllParticipantsSelected()
        {
            if (AllParticipantsList == null || !AllParticipantsList.Any())
            {
                IsAssignToAll = false;
                return;
            }

            bool allAssigned = AllParticipantsList.All(p =>
                _participantAssignedTestService.IsAssigned(currentTestId, p.Id));

            IsAssignToAll = allAssigned;
        }

        private void LoadDatesWithAssignments()
        {
            var dates = _participantAssignedTestService.Assignments
                .Where(a => a.DateTimeAssigned.HasValue)
                .Select(a => a.DateTimeAssigned.Value.Date)
                .Distinct()
                .ToList();

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
    }
}