using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

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

        private readonly ParticipantService _participantService;
        private readonly CuratorService _curatorService;
        private readonly TestService _testService;
        private readonly RequestService _requestService;
        private readonly GroupService _groupService;

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
                if (SetProperty(ref _groupsList, value))
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
                    _ = LoadForCurrentParticipantAsync();
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
                    _ = LoadForCurrentCuratorAsync();

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
                if (SetProperty(ref _selectedGroup, value) && SelectedGroup != null)
                {
                    _ = LoadParticipantsForGroupAsync();
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

        public ICommand SaveParticipantCommand { get; set; }
        public ICommand ArchiveParticipantCommand { get; set; }
        public ICommand ArchiveCuratorCommand { get; set; }
        public ICommand AdminStatusCuratorCommand { get; set; }
        public ICommand ArchiveTestCommand { get; set; }

        public ICommand AcceptRequestCommand { get; set; }
        public ICommand RejectRequestCommand { get; set; }
        public ICommand DeleteGroupCommand { get; set; }


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
            INavigationService navigationService,
            ParticipantService participantService,
            CuratorService curatorService,
            TestService testService,
            RequestService requestService,
            GroupService groupService, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _participantService = participantService;
            _curatorService = curatorService;
            _testService = testService;
            _requestService = requestService;
            _groupService = groupService;

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _participantService.InitializeAsync();
            await _curatorService.InitializeAsync();
            await _requestService.InitializeAsync();
            await _groupService.InitializeAsync();

            ParticipantsList = new ObservableCollection<Participant>(_participantService.Participants);
            CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
            RequestsList = new ObservableCollection<Requests>(_requestService.Requests);
            GroupsList = new ObservableCollection<Group>(_groupService.Groups);

            BindIsChecked = true;
            ActiveIsChecked = true;
            ActiveIsCheckedReq = true;

            SaveParticipantCommand = new RelayCommand(_ => _ = SaveParticipantAsync());
            ArchiveParticipantCommand = new RelayCommand(_ => _ = ArchiveParticipantAsync());
            ArchiveCuratorCommand = new RelayCommand(_ => _ = ArchiveCuratorAsync());
            AdminStatusCuratorCommand = new RelayCommand(_ => _ = AdminStatusCuratorAsync());
            ArchiveTestCommand = new RelayCommand(_ => _ = ArchiveTestAsync());
            AcceptRequestCommand = new RelayCommand(_ => _ = AcceptRequestAsync());
            RejectRequestCommand = new RelayCommand(_ => _ = RejectRequestAsync());
            DeleteGroupCommand = new RelayCommand(_ => _ = DeleteGroupAsync());
        }

        private async Task LoadForCurrentParticipantAsync()
        {
            if (SelectedParticipant != null)
            {
                await _groupService.GetAllGroupsForUserAsync(SelectedParticipant.Id);
                GroupsListCurrent = new ObservableCollection<Group>(_groupService.Groups);

                await _curatorService.GetAllByUserAsync(SelectedParticipant.Id);
                CuratorsListCurrent = new ObservableCollection<Curator>(_curatorService.Curators);
            }
        }

        private async Task LoadForCurrentCuratorAsync()
        {
            if (SelectedCurator != null)
            {
                await _testService.GetAllAsync(SelectedCurator.Id);
                TestsForCuratorList = new ObservableCollection<Test>(_testService.Tests);

                var participants = await _participantService.GetAllBindAsync(SelectedCurator.Id);
                ParticipantsForCuratorList = new ObservableCollection<Participant>(participants);

                await _groupService.GetAllGroupsForCuratorAsync(AdminModeOn, SelectedCurator.Id);
                GroupsForCuratorList = new ObservableCollection<Group>(_groupService.Groups);
            }
        }

        private async Task LoadParticipantsForGroupAsync()
        {
            await _participantService.GetAllParticipantForGroupAsync(SelectedGroup.Id);
            ParticipantsForGroupList = new ObservableCollection<Participant>(_participantService.Participants);
        }

        public async Task SaveParticipantAsync()
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
                await _participantService.UpdateAsync(part);

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

            await _participantService.AddAsync(newP);

            ParticipantsList.Add(newP);
        }

        private async Task ArchiveParticipantAsync()
        {
            if (SelectedParticipant == null) return;
            await _participantService.UpdateParticipantArchiveStatusAsync(SelectedParticipant.Id);
        }

        private async Task ArchiveCuratorAsync()
        {
            if (SelectedCurator == null) return;
            await _curatorService.UpdateCuratorArchiveStatusAsync(SelectedCurator.Id);
        }

        private async Task AdminStatusCuratorAsync()
        {
            if (SelectedCurator == null) return;
            await _curatorService.UpdateCuratorAdminStatusAsync(SelectedCurator.Id);
        }

        private async Task ArchiveTestAsync()
        {
            if (SelectedTest == null) return;
            await _testService.ArchiveTestAsync(SelectedTest.Id);
        }

        private async void ApplyFiltersUsers()
        {
            var queryList = _participantService.Participants.AsEnumerable();

            if (BindIsChecked)
                queryList = await _participantService.GetAllBindAsync(CurrentUser.Id);

            if (CreateIsChecked)
                queryList = queryList.Where(o => o.CuratorCreateId == CurrentUser.Id);

            if (ActiveIsChecked && !ArchiveIsChecked)
                queryList = queryList.Where(o => o.IsArchive == false || o.IsArchive == null);
            else if (ArchiveIsChecked && !ActiveIsChecked)
                queryList = queryList.Where(o => o.IsArchive == true);

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
            var query = _groupService.Groups.AsEnumerable();

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

        public async Task AcceptRequestAsync()
        {
            SelectedRequests.Status = "Принята";
            SelectedRequests.ReviewerId = CurrentUser.Id;
            await _requestService.UpdateAsync(SelectedRequests);

            await _participantService.AddAsync(new Participant()
            {
                Name = Name,
                Login = SelectedRequests.Login,
                Password = SelectedRequests.Password,
                CuratorCreateId = CurrentUser.Id,
                IsArchive = false,
            });
        }

        public async Task RejectRequestAsync()
        {
            SelectedRequests.Status = "Отклонена";
            SelectedRequests.ReviewerId = CurrentUser.Id;
            await _requestService.UpdateAsync(SelectedRequests);
        }

        public async Task DeleteGroupAsync()
        {
            await _groupService.DeleteAsync(SelectedGroup);
            ApplyFiltersGroups();
        }
    }
}