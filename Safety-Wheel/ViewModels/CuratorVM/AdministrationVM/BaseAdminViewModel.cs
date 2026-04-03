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
        // Сервисы
        protected readonly IDialogService _dialogService;
        protected readonly INavigationService _navigationService;

        protected ParticipantService _participantService = new();
        protected CuratorService _curatorService = new();
        protected TestService _testService = new(true);
        protected RequestService _requestService = new();
        protected GroupService _groupService;

        // Общие фильтры...
        private bool _bindIsChecked = false;
        public bool BindIsChecked
        {
            get => _bindIsChecked;
            set
            {
                _bindIsChecked = value;
                OnPropertyChanged();
                ApplyFilters();
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
                ApplyFilters();
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
                ApplyFilters();
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
                ApplyFilters();
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
                ApplyFilters();
            }
        }

        // Конструктор с DI
        protected BaseAdminViewModel(IDialogService dialogService, INavigationService navigationService) : base(navigationService, dialogService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _groupService = new GroupService();
        }

        public abstract void ApplyFilters();
    }
}
