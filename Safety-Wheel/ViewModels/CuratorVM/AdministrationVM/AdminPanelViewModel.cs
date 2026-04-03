using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.ViewModels.CuratorVM
{
    public class AdminPanelViewModel : BaseViewModel
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
                        RequestsVisibility = Visibility.Collapsed;
                        GroupsVisibility = Visibility.Collapsed;
                        break;
                    case TabType.Requests:
                        CuratorsVisibility = Visibility.Collapsed;
                        RequestsVisibility = Visibility.Visible;
                        GroupsVisibility = Visibility.Collapsed;
                        ParticipantsVisibility = Visibility.Collapsed;
                        break;
                    case TabType.Groups:
                        CuratorsVisibility = Visibility.Collapsed;
                        RequestsVisibility = Visibility.Collapsed;
                        GroupsVisibility = Visibility.Visible;
                        ParticipantsVisibility = Visibility.Collapsed;
                        break;
                }
                
                OnPropertyChanged(nameof(SelectedTab));
            }
        }

        private Visibility _curatorsVisibility = Visibility.Collapsed;
        public Visibility CuratorsVisibility
        {
            get => _curatorsVisibility;
            set { _curatorsVisibility = value; OnPropertyChanged(); }
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

        private Visibility _participantsVisibility = Visibility.Collapsed;
        public Visibility ParticipantsVisibility
        {
            get => _participantsVisibility;
            set { _participantsVisibility = value; OnPropertyChanged(); }
        }

        private bool _bindIsChecked = false;
        public bool BindIsChecked
        {
            get => _bindIsChecked;
            set
            {
                _bindIsChecked = value;
                OnPropertyChanged();
                ApplyFiltersUsers();
            }
        }

        private bool _сreateIsChecked = false;
        public bool CreateIsChecked
        {
            get => _сreateIsChecked;
            set
            {
                _сreateIsChecked = value;
                OnPropertyChanged();
                ApplyFiltersUsers();
            }
        }

        private bool _activeIsChecked = false;
        public bool ActiveIsChecked
        {
            get => _activeIsChecked;
            set
            {
                _activeIsChecked = value;
                OnPropertyChanged();
                ApplyFiltersUsers();
            }
        }

        private bool _archiveIsChecked = false;
        public bool ArchiveIsChecked
        {
            get => _archiveIsChecked;
            set
            {
                _archiveIsChecked = value;
                OnPropertyChanged();
                ApplyFiltersUsers();
            }
        }


        private bool _ArchiveIsCheckedReq = false;
        public bool ArchiveIsCheckedReq
        {
            get => _ArchiveIsCheckedReq;
            set
            {
                _ArchiveIsCheckedReq = value;
                OnPropertyChanged();
                ApplyFiltersRequests();
            }
        }

        private bool _ActiveIsCheckedReq = false;
        public bool ActiveIsCheckedReq
        {
            get => _ActiveIsCheckedReq;
            set
            {
                _ActiveIsCheckedReq = value;
                OnPropertyChanged();
                ApplyFiltersRequests();
            }
        }
        
        public string _name;
        public string _login;
        public string _date;

        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public string Login { get { return _login; } set { _login = value; OnPropertyChanged(nameof(Login)); } }
        public string Date { get { return _date; } set { _date = value; OnPropertyChanged(nameof(Date)); } }

        private bool _bindMe = false;
        private bool _bindFor = false;
        private bool _bindNone = false;
        
        public bool BindMe { get { return _bindMe; } set { _bindMe = value; OnPropertyChanged(nameof(BindMe)); } }
        public bool BindFor { get { return _bindFor; } set { _bindFor = value; OnPropertyChanged(nameof(BindFor)); } }
        public bool BindNone { get { return _bindNone; } set { _bindNone = value; OnPropertyChanged(nameof(BindNone)); } }

        private bool _AcceptEnabled = true;
        private bool _RejectEnabled = true;
        public bool AcceptEnabled { get { return _AcceptEnabled; } set { _AcceptEnabled = value; OnPropertyChanged(nameof(AcceptEnabled)); } }
        public bool RejectEnabled { get { return _RejectEnabled; } set { _RejectEnabled = value; OnPropertyChanged(nameof(RejectEnabled)); } }


        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFiltersUsers();
            }
        }
        private string _searchTextRequest;
        public string SearchTextRequest
        {
            get => _searchTextRequest;
            set
            {
                _searchTextRequest = value;
                OnPropertyChanged();
                ApplyFiltersRequests();
            }
        }
        private string _searchTextGroup;
        public string SearchTextGroup
        {
            get => _searchTextGroup;
            set
            {
                _searchTextGroup = value;
                OnPropertyChanged();
                ApplyFiltersGroups();
            }
        }

        ParticipantService _participantService = new();
        CuratorService _curatorService = new();
        TestService _testService = new(true);
        RequestService _requestService = new();
        GroupService _groupService;

        private ObservableCollection<Participant> _participantsList;
        private ObservableCollection<Curator> _curatorsList;
        private ObservableCollection<Requests> _requestsList;
        private ObservableCollection<Group> _groupsList;

        private ObservableCollection<Participant> _participantsForCuratorList;
        private ObservableCollection<Group> _groupsForCuratorList;
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


        private ObservableCollection<Participant> _ParticipantsForGroupList;
        public ObservableCollection<Participant> ParticipantsForGroupList
        {
            get => _ParticipantsForGroupList;
            set
            {
                _ParticipantsForGroupList = value;
                OnPropertyChanged(nameof(ParticipantsForGroupList));
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
        public ObservableCollection<Requests> RequestsList
        {
            get => _requestsList;
            set
            {
                _requestsList = value;
                OnPropertyChanged(nameof(RequestsList));
            }
        }
        
        public ObservableCollection<Group> GroupsList
        {
            get => _groupsList;
            set
            {
                _groupsList = value;
                if(SetProperty(ref _groupsList, value))
                {
                    ApplyFiltersGroups();
                }
            }
        }
        public ObservableCollection<Participant> ParticipantsForCuratorList
        {
            get => _participantsForCuratorList;
            set => SetProperty(ref _participantsForCuratorList, value);

        }
        
        public ObservableCollection<Group> GroupsForCuratorList
        {
            get => _groupsForCuratorList;
            set => SetProperty(ref _groupsForCuratorList, value);

        }
        public ObservableCollection<Test> TestsForCuratorList
        {
            get => _testsForCuratorList;
            set => SetProperty(ref _testsForCuratorList, value);
        }


        private Curator _selectedCurator;
        private Group _selectedGroup;
        private Test _selectedTest;
        private Requests _selectedRequest;
        public Requests SelectedRequests
        {
            get => _selectedRequest;
            set
            {
                if (SetProperty(ref _selectedRequest, value))
                {
                    if (_selectedRequest == null) return;
                    Name = SelectedRequests.Name;
                    Login = SelectedRequests.Login;
                    Date = SelectedRequests.DateTimeApplication.ToString();

                    if (SelectedRequests.Status == "Ожидает подтверждения")
                    {
                        RejectEnabled = true;
                        AcceptEnabled = true;
                    }
                    else
                    {
                        RejectEnabled = false;
                        AcceptEnabled = false;
                    }
                }
            }
        }

        private ObservableCollection<Group> _groupsListCurrent;
        private ObservableCollection<Curator> _curatorsListCurrent;

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                if (SetProperty(ref _selectedParticipant, value))
                {
                    LoadForCurrentParticipant();
                    ParticipantsVisibility = Visibility.Visible;
                    CuratorsVisibility = Visibility.Collapsed;
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
                    LoadForCurrentCurator();

                    CuratorsVisibility = Visibility.Visible;
                    ParticipantsVisibility = Visibility.Collapsed;
                }
            }
        }

        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value) && SelectedGroup!= null)
                {
                    ParticipantsForGroupList = _participantService.GetAllParticipantForGroup(SelectedGroup.Id);
                }
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
        public ObservableCollection<Curator> CuratorsListCurrent
        {
            get => _curatorsListCurrent;
            set => SetProperty(ref _curatorsListCurrent, value);
        }

        public RelayCommand SaveParticipantCommand { get; }
        public RelayCommand ArchiveParticipantCommand { get; }
        public RelayCommand ArchiveCuratorCommand { get; }
        public RelayCommand AdminStatusCuratorCommand { get; }
        public RelayCommand ArchiveTestCommand { get; }

        public RelayCommand AcceptRequestCommand { get; }
        public RelayCommand RejectRequestCommand { get; }
        public RelayCommand DeleteGroupCommand { get; }


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
        
        public AdminPanelViewModel(
            IDialogService dialogService,
            INavigationService navigationService) : base(navigationService, dialogService)
        {
            ParticipantsList = new ObservableCollection<Participant>(_participantService.Participants);
            CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
            RequestsList = new ObservableCollection<Requests>(_requestService.Requests);
            _groupService = new();
            _groupService.GetAllGroupsForUser();
            GroupsList = new ObservableCollection<Group>(_groupService.Group);

            BindIsChecked = true;
            ActiveIsChecked = true;
            ActiveIsCheckedReq = true;

            SaveParticipantCommand = new RelayCommand(_ => SaveParticipant());

            ArchiveParticipantCommand = new RelayCommand(_ => ArchiveParticipant());
            ArchiveCuratorCommand = new RelayCommand(_ => ArchiveCurator());
            AdminStatusCuratorCommand = new RelayCommand(_ => AdminStatusCurator());

            ArchiveTestCommand = new RelayCommand(_ => ArchiveTest());

            AcceptRequestCommand = new RelayCommand(_ => AcceptRequest());
            RejectRequestCommand = new RelayCommand(_ => RejectRequest());
            DeleteGroupCommand = new RelayCommand(_ => DeleteGroup());

            //LoadGroupsForCurrentParticipant();
        }

        private void LoadForCurrentParticipant()
        {
            if (SelectedParticipant != null)
            {
                _groupService.GetAllGroupsForUser(SelectedParticipant.Id);
                GroupsListCurrent = new ObservableCollection<Group>(_groupService.Group);

                _curatorService.GetAll(SelectedParticipant.Id);
                CuratorsListCurrent = new ObservableCollection<Curator>(_curatorService.Curators);
            }
        }
        private void LoadForCurrentCurator()
        {
            if (SelectedCurator != null)
            {
                _testService.GetAll(SelectedCurator.Id);
                TestsForCuratorList = new ObservableCollection<Test>(_testService.Tests);

                ParticipantsForCuratorList = new ObservableCollection<Participant>(_participantService.GetAllBind(teacherId: SelectedCurator.Id));

                _groupService.GetAllGroupsForCurator(SelectedCurator.Id);
                GroupsForCuratorList = new ObservableCollection<Group>(_groupService.Group);
            }
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
                    IsArchive = _selectedParticipant.IsArchive,
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
                IsArchive = false,
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
        private void ApplyFiltersUsers()
        {
            var queryList = _participantService.Participants.AsEnumerable();
            var queryListBind = _participantService.GetAllBind(CurrentUser.Id);


            if (BindIsChecked)
                queryList = queryListBind;

            if (CreateIsChecked)
                queryList = queryList.Where(o => o.CuratorCreateId == CurrentUser.Id);

            if (ActiveIsChecked && !ArchiveIsChecked)
                queryList = queryList.Where(o => o.IsArchive == false || o.IsArchive == null);
            else if (ArchiveIsChecked && !ActiveIsChecked)
                queryList = queryList.Where(o => o.IsArchive == true );

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                queryList = queryList.Where(p =>
                    p.Name != null &&
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            ParticipantsList.Clear();
            foreach (var participant in queryList)
            {
                ParticipantsList.Add(participant);
            }
        }

        public void ApplyFiltersRequests()
        {
            var query = _requestService.Requests.AsEnumerable();

            if (ActiveIsCheckedReq && !ArchiveIsCheckedReq)
                query = query.Where(o => o.Status == "Ожидает подтверждения");
            else if (ArchiveIsCheckedReq && !ActiveIsCheckedReq)
                query = query.Where(o => o.Status != "Ожидает подтверждения");

            if (!string.IsNullOrWhiteSpace(SearchTextRequest))
            {
                query = query.Where(p =>
                    p.Name != null &&
                    p.Name.Contains(SearchTextRequest, StringComparison.OrdinalIgnoreCase));
            }

            RequestsList.Clear();
            foreach (var r in query)
            {
                RequestsList.Add(r);
            }
        }
        public void ApplyFiltersGroups()
        {
            var query = _groupService.Group.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTextGroup))
            {
                query = query.Where(p =>
                    p.Name != null &&
                    p.Name.Contains(SearchTextGroup, StringComparison.OrdinalIgnoreCase));
            }

            GroupsList.Clear();
            foreach (var r in query)
            {
                GroupsList.Add(r);
            }
        }

        public void AcceptRequest()
        {
            SelectedRequests.Status = "Принята";
            SelectedRequests.ReviewerId = CurrentUser.Id;
            _requestService.Update(SelectedRequests);

            _participantService.Add(new Participant()
            {
                Name = Name,
                Login = SelectedRequests.Login,
                Password = SelectedRequests.Password,
                CuratorCreateId = CurrentUser.Id,
                IsArchive = false,
            });
        }
        public void RejectRequest()
        {
            SelectedRequests.Status = "Отклонена";
            SelectedRequests.ReviewerId = CurrentUser.Id;
            _requestService.Update(SelectedRequests);
        }
        public void DeleteGroup()
        {
            _groupService.Delete(SelectedGroup);
            ApplyFiltersGroups();

        }
    }
}
