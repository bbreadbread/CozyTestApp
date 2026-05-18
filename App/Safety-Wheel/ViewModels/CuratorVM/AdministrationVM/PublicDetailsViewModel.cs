using CozyTest;
using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Collections.ObjectModel;
using System.Windows;
using static OfficeOpenXml.ExcelErrorValue;
using System.Windows.Navigation;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class PublicDetailsViewModel : BaseViewModel
    {
        public override string WindowTitle => "Настройка публикации теста";

        public int currentTestId = 0;
        GroupService _groupService;
        ParticipantService _participantService;
        ParticipantPublicTestService _participantPublicTestService;
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

        public RelayCommand AllBindCommand { get; }
        public RelayCommand GroupsBindCommand { get; }
        public RelayCommand ParticipantsBindCommand { get; }

        public RelayCommand SwitchPublisedGroupsCommand { get; }
        public RelayCommand SwitchPublisedParticipantCommand { get; }
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

        private Visibility _groupsDataGridVisibility = Visibility.Visible;
        public Visibility GroupsDataGridVisibility
        {
            get => _groupsDataGridVisibility;
            set { _groupsDataGridVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _participantsDataGridVisibility = Visibility.Collapsed;
        public Visibility ParticipantsDataGridVisibility
        {
            get => _participantsDataGridVisibility;
            set { _participantsDataGridVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _allParticipantsDataGridVisibility = Visibility.Collapsed;
        public Visibility AllParticipantsDataGridVisibility
        {
            get => _allParticipantsDataGridVisibility;
            set { _allParticipantsDataGridVisibility = value; OnPropertyChanged(); }
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

        private bool _isAllPublic;
        public bool IsAllPublic
        {
            get => _isAllPublic;
            set
            {
                _isAllPublic = value;
                SetProperty(ref _isAllPublic, value);
            }
        }

        public PublicDetailsViewModel(
            IDialogService dialogService,
        INavigationService navigationService,
        ParticipantService participantService,
            GroupService groupService,
            ParticipantPublicTestService participantPublicTestService) : base(navigationService, dialogService)
        {
            CurrentUser.AdminModeOnChanged += async (_, _) =>
            {
                OnPropertyChanged(nameof(AdminModeOn));
                await RefreshDataAsync();
            };

            _groupService = groupService;
            _participantService = participantService;
            _participantPublicTestService = participantPublicTestService;
            _currentTest = null;

            AllBindCommand = new RelayCommand(_ => AllBind());
            GroupsBindCommand = new RelayCommand(_ => GroupsBind());
            ParticipantsBindCommand = new RelayCommand(_ => ParticipantsBind());

            SwitchPublisedGroupsCommand = new RelayCommand(_ => SwitchPublisedGroups());
            SwitchPublisedParticipantCommand = new RelayCommand(_ => SwitchPublisedParticipant());
            SwitchAllParticipantCommand = new RelayCommand(_ => SwitchAllParticipant());

            Task.Run(async () => await LoadDataAsync());
        }

        public PublicDetailsViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ParticipantService participantService,
        GroupService groupService,
            ParticipantPublicTestService participantPublicTestService,
            Test test) : base(navigationService, dialogService)
        {
            _groupService = groupService;
            _participantService = participantService;
            _participantPublicTestService = participantPublicTestService;
            _currentTest = test;
            currentTestId = test?.Id ?? 0;

            AllBindCommand = new RelayCommand(_ => AllBind());
            GroupsBindCommand = new RelayCommand(_ => GroupsBind());
            ParticipantsBindCommand = new RelayCommand(_ => ParticipantsBind());

            SwitchPublisedGroupsCommand = new RelayCommand(_ => SwitchPublisedGroups());
            SwitchPublisedParticipantCommand = new RelayCommand(_ => SwitchPublisedParticipant());
            SwitchAllParticipantCommand = new RelayCommand(_ => SwitchAllParticipant());

            Task.Run(async () => await LoadDataAsync());
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                await _participantService.GetAllParticipantsAsync(AdminModeOn, CurrentUser.Id);
                await _participantPublicTestService.GetAllAsync(currentTestId);

                var groups = await _groupService.GetAllGroupsForCuratorAsync(AdminModeOn, CurrentUser.Id, currentTestId);
                var allParticipants = await _participantService.GetAllAsync(CurrentUser.Id);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GroupsList = new ObservableCollection<Group>(groups);
                    ParticipantsList = _participantService.Participants;
                    AllParticipantsList = new ObservableCollection<Participant>(allParticipants);

                    UpdateGroupPublicationStatus();
                    UpdateParticipantsPublicationStatus();

                    CheckIfAllParticipantsSelected();
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
                UpdateGroupPublicationStatus();
            }
            else if (SelectedTabIndex == 1)
            {
                UpdateParticipantsPublicationStatus();
            }
        }

        private async Task UpdateGroupPublicationStatus()
        {
            if (GroupsList == null) return;

            foreach (var group in GroupsList)
            {
                var participantsInGroup = await _groupService.GetAllParticipantForGroup(group.Id);
                bool allPublished = participantsInGroup.Any() && participantsInGroup.All(p =>
                    _participantPublicTestService.IsPublished(currentTestId, p.Id));

                group.IsPublished = allPublished;
            }
        }

        private void UpdateParticipantsPublicationStatus()
        {
            if (ParticipantsList == null) return;

            foreach (var participant in ParticipantsList)
            {
                participant.IsPublished = _participantPublicTestService.IsPublished(currentTestId, participant.Id);
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
            if (IsAllPublic == false) await _participantPublicTestService.SwitchParticipantPublicStatusAsync(currentTestId, allParticipants, true);
            else await _participantPublicTestService.SwitchParticipantPublicStatusAsync(currentTestId, allParticipants, false);

            IsAllPublic = !IsAllPublic;
            await RefreshDataAsync();
        }

        public void GroupsBind()
        {
            SelectedTabIndex = 0;
            IsAllPublic = false;
        }

        public void ParticipantsBind()
        {
            SelectedTabIndex = 1;
            IsAllPublic = false;
        }

        public async void SwitchPublisedGroups()
        {
            if (SelectedGroup == null) return;

            var participantsInGroup = await _groupService.GetAllParticipantForGroup(SelectedGroup.Id);

            if (!participantsInGroup.Any())
            {
                MessageBox.Show("В группе нет участников");
                return;
            }

            bool currentStatus = participantsInGroup.All(p =>
                _participantPublicTestService.IsPublished(currentTestId, p.Id));

            bool newStatus = !currentStatus;

            foreach (var participant in participantsInGroup)
            {
                await _participantPublicTestService.SwitchParticipantPublicStatusAsync(participant.Id, currentTestId);
            }

            SelectedGroup.IsPublished = newStatus;

            await RefreshDataAsync();
        }

        public async void SwitchPublisedParticipant()
        {
            if (SelectedParticipant == null) return;

            await _participantPublicTestService.SwitchParticipantPublicStatusAsync(SelectedParticipant.Id, currentTestId);

            await RefreshDataAsync();
        }

        public async void SwitchAllParticipant()
        {
            if (SelectedAllParticipant == null) return;

            await _participantPublicTestService.SwitchParticipantPublicStatusAsync(SelectedAllParticipant.Id, currentTestId);

            await RefreshDataAsync();
        }

        private async Task RefreshDataAsync()
        {
            await _participantService.GetAllParticipantsAsync(AdminModeOn, CurrentUser.Id);
            await _participantPublicTestService.GetAllAsync(currentTestId);

            var groups = await _groupService.GetAllGroupsForCuratorAsync(AdminModeOn, CurrentUser.Id, currentTestId);
            var allParticipants = await _participantService.GetAllAsync(CurrentUser.Id);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                GroupsList = new ObservableCollection<Group>(groups);
                ParticipantsList = _participantService.Participants;
                AllParticipantsList = new ObservableCollection<Participant>(allParticipants);

                UpdateGroupPublicationStatus();
                UpdateParticipantsPublicationStatus();

                CheckIfAllParticipantsSelected();
            });
        }

        private void CheckIfAllParticipantsSelected()
        {
            if (AllParticipantsList == null || !AllParticipantsList.Any())
            {
                IsAllPublic = false;
                return;
            }

            bool allPublished = AllParticipantsList.All(p =>
                _participantPublicTestService.IsPublished(currentTestId, p.Id));

            IsAllPublic = allPublished;
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }
    }
}