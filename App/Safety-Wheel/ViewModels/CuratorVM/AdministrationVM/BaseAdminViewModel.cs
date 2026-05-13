using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public abstract class BaseAdminViewModel : BaseViewModel
    {
        protected readonly IDialogService _dialogService;
        protected readonly INavigationService _navigationService;

        protected readonly ParticipantService _participantService;
        protected readonly CuratorService _curatorService;
        protected readonly TestService _testService;
        protected readonly RequestService _requestService;
        protected readonly GroupService _groupService;

        private bool _adminModeOn = false;
        public bool AdminModeOn
        {
            get => _adminModeOn;
            set
            {
                _adminModeOn = value;
                OnPropertyChanged();
                _ = ApplyFiltersAsync();
            }
        }
        private bool _bindIsChecked = false;
        public bool BindIsChecked
        {
            get => _bindIsChecked;
            set
            {
                _bindIsChecked = value;
                OnPropertyChanged();
                _ = ApplyFiltersAsync();
            }
        }

        private bool _createIsChecked = false;
        public bool CreateIsChecked
        {
            get => _createIsChecked;
            set
            {
                _createIsChecked = value;
                OnPropertyChanged();
                _ = ApplyFiltersAsync();
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
                _ = ApplyFiltersAsync();
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
                _ = ApplyFiltersAsync();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                _ = ApplyFiltersAsync();
            }
        }

        protected BaseAdminViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ParticipantService participantService,
            CuratorService curatorService,
            TestService testService,
            RequestService requestService,
            GroupService groupService) : base(navigationService, dialogService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _participantService = participantService;
            _curatorService = curatorService;
            _testService = testService;
            _requestService = requestService;
            _groupService = groupService;
        }

        public abstract Task ApplyFiltersAsync();
    }
}