using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class PublicDetailsViewModel : BaseViewModel
    {
        public override string WindowTitle => "Настройка публикации теста";

        public int currentTestId = 0;
        GroupService _groupService = new();
        ParticipantService _participantService = new();
        ParticipantPublicTestService _participantPublicTestService = new();

        private ObservableCollection<Group> _groupsList;
        public ObservableCollection<Group> GroupsList
        {
            get
            {
                _groupsList = _groupService.GetAllGroupsForCurator(CurrentUser.Id, currentTestId);
                return _groupsList;
            }
            set
            {
                _groupsList = value;
                OnPropertyChanged(nameof(GroupsList));
            }
        }

        private ObservableCollection<Participant> _participantsList;
        public ObservableCollection<Participant> ParticipantsList
        {
            get
            {
                _participantsList = _participantService.Participants;
                return _participantsList;
            }
            set
            {
                _participantsList = value;
                OnPropertyChanged(nameof(GroupsList));
            }
        }

        public RelayCommand AllBindCommand { get; }
        public RelayCommand GroupsBindCommand { get; }
        public RelayCommand ParticipantsBindCommand { get; }

        public RelayCommand SwitchPublisedGroupsCommand { get; }
        public RelayCommand SwitchPublisedParticipantCommand { get; }

        private Visibility _groupsDataGridVisibility = Visibility.Collapsed;
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

        private bool _isAllBindSelected;
        public bool IsAllBindSelected
        {
            get => _isAllBindSelected;
            set { _isAllBindSelected = value; OnPropertyChanged(); }
        }

        private bool _isGroupsBindSelected;
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

        private bool _isEnabledGroup = true;
        public bool IsEnabledGroup
        {
            get => _isEnabledGroup;
            set { _isEnabledGroup = value; OnPropertyChanged(); }
        }

        private bool _isEnabledParticipant = true;
        public bool IsEnabledParticipant
        {
            get => _isEnabledParticipant;
            set { _isEnabledParticipant = value; OnPropertyChanged(); }
        }

        private bool _isAllPublic;
        public bool IsAllPublic
        {
            get => _isAllPublic;
            set
            {
                _isAllPublic = value;
                OnPropertyChanged();

                if (IsAllPublic == true)
                {
                    IsEnabledGroup = false;
                    IsEnabledParticipant = false;
                }
                else
                {
                    IsEnabledGroup = true;
                    IsEnabledParticipant = true;
                }

            }
        }

        public PublicDetailsViewModel(
            IDialogService dialogService,
            INavigationService navigationService) : base(navigationService, dialogService)
        {
            GroupsList = new ObservableCollection<Group>(_groupService.GetAllGroupsForCurator(CurrentUser.Id, currentTestId));
            AllBindCommand = new RelayCommand(_ => AllBind());
            GroupsBindCommand = new RelayCommand(_ => GroupsBind());
            ParticipantsBindCommand = new RelayCommand(_ => ParticipantsBind());

            SwitchPublisedGroupsCommand = new RelayCommand(_ => SwitchPublisedGroups());
            SwitchPublisedParticipantCommand = new RelayCommand(_ => SwitchPublisedParticipant());

            _participantPublicTestService.GetAll(currentTestId);

            DetermineInitialBindState();
        }

        public void AllBind()
        {
            var participants = _participantService.GetAll(CurrentUser.Id);

            if (!participants.Any())
            {
                MessageBox.Show("Нет привязанных к Вам тестируемых");
                IsAllBindSelected = false;
                return;
            }

            IsEnabledGroup = false;
            IsEnabledParticipant = false;

            IsGroupsBindSelected = false;
            IsParticipantsBindSelected = false;
            IsAllPublic = true;

            GroupsDataGridVisibility = Visibility.Collapsed;
            ParticipantsDataGridVisibility = Visibility.Collapsed;

            _participantPublicTestService.SwitchParticipantPublicStatus(currentTestId, participants);
        }

        public void GroupsBind()
        {
            IsEnabledGroup = true;
            IsEnabledParticipant = true;
            IsAllPublic = false;

            GroupsDataGridVisibility = Visibility.Visible;
            ParticipantsDataGridVisibility = Visibility.Collapsed;

            IsAllBindSelected = false;
            IsParticipantsBindSelected = false;
        }

        public void ParticipantsBind()
        {
            IsEnabledGroup = true;
            IsEnabledParticipant = true;
            IsAllPublic = false;

            GroupsDataGridVisibility = Visibility.Collapsed;
            ParticipantsDataGridVisibility = Visibility.Visible;

            IsAllBindSelected = false;
            IsGroupsBindSelected = false;
        }

        public void SwitchPublisedGroups()
        {
            if (SelectedGroup == null) return;

            var participants = _groupService.GetAllParticipantForGroup(SelectedGroup.Id);

            if (!participants.Any())
            {
                MessageBox.Show("В группе нет участников");
                return;
            }

            _participantPublicTestService.SwitchParticipantPublicStatus(currentTestId, SelectedGroup.Id, participants);
            SelectedGroup.IsPublished = !SelectedGroup.IsPublished;

            _participantService.GetAllParticipants();
            ParticipantsList = _participantService.Participants;

            CheckIfAllParticipantsSelected();
        }

        public void SwitchPublisedParticipant()
        {
            if (SelectedGroup != null)
            {
                _groupService.GetAllGroupsForCurator(SelectedGroup.Id, currentTestId);
            }

            _participantPublicTestService.SwitchParticipantPublicStatus(SelectedParticipant.Id, currentTestId);
            GroupsList = _groupService.GetAllGroupsForCurator(CurrentUser.Id, currentTestId);

            CheckIfAllParticipantsSelected();
        }

        private void CheckIfAllParticipantsSelected()
        {
            var allParticipants = _participantService.GetAll(CurrentUser.Id);

            if (!allParticipants.Any())
                return;

            bool allPublished = allParticipants.All(p =>
                _participantPublicTestService.IsPublished(currentTestId, p.Id));

            IsAllPublic = allPublished;
        }

        private void DetermineInitialBindState()
        {
            var participants = _participantService.GetAll(CurrentUser.Id);
            var groups = _groupService.GetAllGroupsForCurator(CurrentUser.Id, currentTestId);

            bool allParticipantsPublished = participants.Any() &&
                participants.All(p => _participantPublicTestService.IsPublished(currentTestId, p.Id));

            bool hasGroupBindings = groups.Any(g => g.IsPublished);
            bool hasIndividualBindings = participants.Any(p =>
                _participantPublicTestService.IsPublished(currentTestId, p.Id)) && !allParticipantsPublished;

            if (allParticipantsPublished)
            {
                IsAllPublic = true;
                IsAllBindSelected = true;
                IsEnabledGroup = true;
                IsEnabledParticipant = true;
            }
            else if (hasGroupBindings)
            {
                IsAllBindSelected = false;
                IsGroupsBindSelected = true;
                IsParticipantsBindSelected = false;
                IsAllPublic = false;
                IsEnabledGroup = true;
                IsEnabledParticipant = true;

                GroupsBind();
            }
            else if (hasIndividualBindings)
            {
                IsAllBindSelected = false;
                IsGroupsBindSelected = false;
                IsParticipantsBindSelected = true;
                IsAllPublic = false;
                IsEnabledGroup = true;
                IsEnabledParticipant = true;

                ParticipantsBind();
            }
            else
            {
                IsAllBindSelected = false;
                IsGroupsBindSelected = false;
                IsParticipantsBindSelected = false;
                IsAllPublic = false;
                IsEnabledGroup = true;
                IsEnabledParticipant = true;
            }
        }
    }
}