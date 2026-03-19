using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.ViewModels.CuratorVM
{
    public class AdminPanelViewModel : ObservableObject
    {

        public enum TabType { Users, Requests, Groups }

        private TabType _selectedTab = TabType.Users;

        public TabType SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                switch (_selectedTab)
                {
                    case TabType.Users:
                        UsersVisibility = Visibility.Visible;
                        RequestsVisibility = Visibility.Collapsed;
                        GroupsVisibility = Visibility.Collapsed;
                        break;
                    case TabType.Requests:
                        UsersVisibility = Visibility.Collapsed;
                        RequestsVisibility = Visibility.Visible;
                        GroupsVisibility = Visibility.Collapsed;
                        break;
                    case TabType.Groups:
                        UsersVisibility = Visibility.Collapsed;
                        RequestsVisibility = Visibility.Collapsed;
                        GroupsVisibility = Visibility.Visible;
                        break;
                }
                
                OnPropertyChanged(nameof(SelectedTab));
            }
        }

        private Visibility _usersVisibility = Visibility.Collapsed;
        public Visibility UsersVisibility
        {
            get => _usersVisibility;
            set { _usersVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _requestsVisibility = Visibility.Collapsed;
        public Visibility RequestsVisibility
        {
            get => _requestsVisibility;
            set { _requestsVisibility = value; OnPropertyChanged(); }

        }

        private Visibility _groupsVisibility = Visibility.Collapsed;
        public Visibility GroupsVisibility
        {
            get => _groupsVisibility;
            set { _groupsVisibility = value; OnPropertyChanged(); }

        }

        ParticipantService _participantService = new();
        CuratorService _curatorService = new();
        RequestService _applicationsService = new();
        GroupService _groupService = new();
        TestService _testService = new(true);

        private ObservableCollection<Participant> _participantsList;
        private ObservableCollection<Curator> _curatorsList;
        private ObservableCollection<Requests> _applicationsList;
        private ObservableCollection<Group> _groupsList;

        private ObservableCollection<Participant> _participantsForCuratorList;
        private ObservableCollection<Test> _testsForCuratorList;

        public ObservableCollection<Participant> ParticipantsList
        {
            get => _participantsList;
            set
            {
                _participantsList = value;
                OnPropertyChanged(nameof(ParticipantsList));
            }
        }
        public ObservableCollection<Curator> CuratorsList
        {
            get => _curatorsList;
            set
            {
                _curatorsList = value;
                OnPropertyChanged(nameof(CuratorsList));
            }
        }
        public ObservableCollection<Requests> ApplicationsList
        {
            get => _applicationsList;
            set
            {
                _applicationsList = value;
                OnPropertyChanged(nameof(ApplicationsList));
            }
        }
        public ObservableCollection<Group> GroupsList
        {
            get => _groupsList;
            set
            {
                _groupService.GetAllGroupsForUser();
                _groupsList = _groupService.Group;
                SetProperty(ref _groupsList, value);
            }
        }
        public ObservableCollection<Participant> ParticipantsForCuratorList
        {
            get => _participantsForCuratorList;
            set => SetProperty(ref _participantsForCuratorList, value);

        }
        public ObservableCollection<Test> TestsForCuratorList
        {
            get => _testsForCuratorList;
            set => SetProperty(ref _testsForCuratorList, value);
        }

        public Group? CurrentGroup { get; set; } = null;

        private Participant _selectedParticipant;
        private Curator _selectedCurator;
        private Participant _selectedCuratorComboBox;
        private Test _selectedTest;

        private ObservableCollection<Group> _groupsListCurrent;

        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                if (SetProperty(ref _selectedParticipant, value))
                {
                    LoadGroupsForCurrentParticipant();
                }
            }
        }
        public Curator SelectedCurator
        {
            get => _selectedCurator;
            set
            {
                if (SetProperty(ref _selectedCurator, value))
                {
                    _testService.GetAll(teacherId: CurrentUser.Id);
                    TestsForCuratorList = new ObservableCollection<Test>(_testService.Tests);
                }
            }
        }
        public Participant SelectedCuratorComboBox
        {
            get => _selectedCuratorComboBox;
            set
            {
                SetProperty(ref _selectedCuratorComboBox, value);
            }
        }
        public Test SelectedTest
        {
            get => _selectedTest;
            set
            {
                SetProperty(ref _selectedTest, value);
            }
        }
        public ObservableCollection<Group> GroupsListCurrent
        {
            get => _groupsListCurrent;
            set => SetProperty(ref _groupsListCurrent, value);
        }

        public RelayCommand SaveParticipantCommand { get; }

        public RelayCommand ArchiveParticipantCommand { get; }
        public RelayCommand ArchiveCuratorCommand { get; }
        public RelayCommand AdminStatusCuratorCommand { get; }
        public RelayCommand ArchiveTestCommand { get; }

        private string _nameParticipant;
        private string _loginParticipant;
        private string _passwordParticipant;
        
        public string NameParticipant
        {
            get => _nameParticipant;
            set => SetProperty(ref _nameParticipant, value);
        }
        public string LoginParticipant
        {
            get => _loginParticipant;
            set => SetProperty(ref _loginParticipant, value);
        }
        public string PasswordParticipant
        {
            get => _passwordParticipant;
            set => SetProperty(ref _passwordParticipant, value);
        }
        
        public AdminPanelViewModel()
        {
            ParticipantsList = new ObservableCollection<Participant>(_participantService.Participants);
            CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
            ApplicationsList = new ObservableCollection<Requests>(_applicationsService.Requests);
            GroupsList = new ObservableCollection<Group>(_groupService.Group);

            SaveParticipantCommand = new RelayCommand(_ => SaveParticipant());

            ArchiveParticipantCommand = new RelayCommand(_ => ArchiveParticipant());
            ArchiveCuratorCommand = new RelayCommand(_ => ArchiveCurator());
            AdminStatusCuratorCommand = new RelayCommand(_ => AdminStatusCurator());

            ArchiveTestCommand = new RelayCommand(_ => ArchiveTest());

            LoadGroupsForCurrentParticipant();
        }

        private void LoadGroupsForCurrentParticipant()
        {
            if (SelectedParticipant?.Id == null)
            {
                GroupsListCurrent = new ObservableCollection<Group>();
                return;
            }

            _groupService.GetAllGroupsForUser(SelectedParticipant.Id);
            GroupsListCurrent = new ObservableCollection<Group>(_groupService.Group);
        }

        public void SaveParticipant()
        {
            if (SelectedParticipant != null)
            {
                var part = new Participant()
                {
                    Id = SelectedParticipant.Id,
                    Name = NameParticipant,
                    Login = LoginParticipant,
                    Password = PasswordParticipant,
                    CuratorCreateId = _selectedParticipant.CuratorCreateId,
                    CuratorCreate = _selectedParticipant.CuratorCreate,
                };
                _participantService.Update(part);

                SelectedParticipant.Name = NameParticipant;
                SelectedParticipant.Login = LoginParticipant;
                SelectedParticipant.Password = PasswordParticipant;

                return;
            }

            var newP = new Participant()
            {
                Name = NameParticipant,
                Login = LoginParticipant,
                Password = PasswordParticipant,
                CuratorCreateId = CurrentUser.Id,
                CuratorCreate = (Curator)CurrentUser.ClassUser,
            };

            _participantService.Add(newP);

            ParticipantsList.Add(newP);
        }

        private void ArchiveParticipant()
        {
            if (SelectedParticipant == null) return;
            _participantService.UpdateParticipantArchiveStatus(SelectedParticipant.Id);
        }
        private void ArchiveCurator()
        {
            if (SelectedCurator == null) return;
            _curatorService.UpdateCuratorArchiveStatus(SelectedCurator.Id);
        }
        private void AdminStatusCurator()
        {
            if (SelectedCurator == null) return;
            _curatorService.UpdateCuratorAdminStatus(SelectedCurator.Id);
        }
        private void ArchiveTest()
        {
            if (SelectedTest == null) return;
            _testService.ArchiveTest(SelectedTest.Id);
        }
    }
}
