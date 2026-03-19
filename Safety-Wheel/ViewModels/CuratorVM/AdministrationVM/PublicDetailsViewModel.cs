using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.ViewModels.CuratorVM
{
    public class PublicDetailsViewModel : ObservableObject
    {
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
            set
            {
                SetProperty(ref _selectedParticipant, value);
            }
        }

        private Group _selectedGroup;
        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                SetProperty(ref _selectedGroup, value);
            }
        }

        public PublicDetailsViewModel()
        {
            GroupsList = new ObservableCollection<Group>(_groupService.GetAllGroupsForCurator(CurrentUser.Id, currentTestId));
            AllBindCommand = new RelayCommand(_ => AllBind());
            GroupsBindCommand = new RelayCommand(_ => GroupsBind());
            ParticipantsBindCommand = new RelayCommand(_ => ParticipantsBind());

            SwitchPublisedGroupsCommand = new RelayCommand(_ => SwitchPublisedGroups());
            SwitchPublisedParticipantCommand = new RelayCommand(_ => SwitchPublisedParticipant());

            _participantPublicTestService.GetAll(currentTestId);
        }

        public void AllBind()
        {
            GroupsDataGridVisibility = System.Windows.Visibility.Collapsed;
            ParticipantsDataGridVisibility = System.Windows.Visibility.Collapsed;
        }
        public void GroupsBind()
        {
            GroupsDataGridVisibility = System.Windows.Visibility.Visible;
            ParticipantsDataGridVisibility = System.Windows.Visibility.Collapsed;
        }
        public void ParticipantsBind()
        {
            GroupsDataGridVisibility = System.Windows.Visibility.Collapsed;
            ParticipantsDataGridVisibility = System.Windows.Visibility.Visible;
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
        }
        public void SwitchPublisedParticipant()
        {
            if (SelectedGroup != null)
            {
                _groupService.GetAllGroupsForCurator(SelectedGroup.Id, currentTestId);
            }
            
            _participantPublicTestService.SwitchParticipantPublicStatus(SelectedParticipant.Id ,currentTestId);

            GroupsList = _groupService.GetAllGroupsForCurator(CurrentUser.Id, currentTestId);
        }
    }
}
