using CozyTest.Models;
using CozyTest.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class RequestsViewModel : BaseAdminViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        private readonly RequestService _requestService = new();
        private readonly ParticipantService _participantService = new();
        private readonly CuratorService _curatorService = new();

        private ObservableCollection<Requests> _requestsList;
        public ObservableCollection<Requests> RequestsList
        {
            get => _requestsList;
            set
            {
                _requestsList = value;
                OnPropertyChanged(nameof(RequestsList));
            }
        }

        private ObservableCollection<Curator> _curatorsList;
        public ObservableCollection<Curator> CuratorsList
        {
            get => _curatorsList;
            set => SetProperty(ref _curatorsList, value);
        }

        private bool _activeIsCheckedReq = true;
        public bool ActiveIsCheckedReq
        {
            get => _activeIsCheckedReq;
            set
            {
                _activeIsCheckedReq = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        private bool _archiveIsCheckedReq = false;
        public bool ArchiveIsCheckedReq
        {
            get => _archiveIsCheckedReq;
            set
            {
                _archiveIsCheckedReq = value;
                OnPropertyChanged();
                ApplyFilters();
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
                ApplyFilters();
            }
        }

        private Requests _selectedRequests;
        public Requests SelectedRequests
        {
            get => _selectedRequests;
            set
            {
                if (SetProperty(ref _selectedRequests, value))
                {
                    OnSelectedRequestChanged();
                }
            }
        }

        private Visibility _requestsVisibility = Visibility.Collapsed;
        public Visibility RequestsVisibility
        {
            get => _requestsVisibility;
            set => SetProperty(ref _requestsVisibility, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _login;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(nameof(Login)); }
        }

        private string _date;
        public string Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(nameof(Date)); }
        }

        private Curator _selectedCuratorForBind;
        public Curator SelectedCuratorForBind
        {
            get => _selectedCuratorForBind;
            set => SetProperty(ref _selectedCuratorForBind, value);
        }

        private bool _bindMe =false;
        public bool BindMe
        {
            get => _bindMe;
            set { _bindMe = value; OnPropertyChanged(nameof(BindMe)); }
        }

        private bool _bindFor = false;
        public bool BindFor
        {
            get => _bindFor;
            set { _bindFor = value; OnPropertyChanged(nameof(BindFor)); }
        }

        private bool _bindNone =  true;
        public bool BindNone
        {
            get => _bindNone;
            set { _bindNone = value; OnPropertyChanged(nameof(BindNone)); }
        }

        private bool _acceptEnabled = false;
        public bool AcceptEnabled
        {
            get => _acceptEnabled;
            set { _acceptEnabled = value; OnPropertyChanged(nameof(AcceptEnabled)); }
        }

        private bool _rejectEnabled = false;
        public bool RejectEnabled
        {
            get => _rejectEnabled;
            set { _rejectEnabled = value; OnPropertyChanged(nameof(RejectEnabled)); }
        }

        public RelayCommand AcceptRequestCommand { get; }
        public RelayCommand RejectRequestCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public RequestsViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;

            RequestsList = new ObservableCollection<Requests>(_requestService.Requests);
            CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);
            ActiveIsCheckedReq = true;

            AcceptRequestCommand = new RelayCommand(_ => AcceptRequest(), _ => CanAcceptOrReject());
            RejectRequestCommand = new RelayCommand(_ => RejectRequest(), _ => CanAcceptOrReject());
            RefreshCommand = new RelayCommand(_ => RefreshData());
        }

        private void OnSelectedRequestChanged()
        {
            if (SelectedRequests == null)
            {
                RequestsVisibility = Visibility.Collapsed;
                return;
            }

            RequestsVisibility = Visibility.Visible;
            Name = SelectedRequests.Name;
            Login = SelectedRequests.Login;
            Date = SelectedRequests.DateTimeApplication.ToString();

            bool isPending = SelectedRequests.Status == "Ожидает подтверждения";
            AcceptEnabled = isPending;
            RejectEnabled = isPending;

            BindMe = false;
            BindFor = false;
            BindNone = true;
            SelectedCuratorForBind = null;
        }

        private bool CanAcceptOrReject()
        {
            return SelectedRequests != null && SelectedRequests.Status == "Ожидает подтверждения";
        }

        private void RefreshData()
        {
            ApplyFilters();
            _dialogService.ShowMessage("Данные обновлены");
        }

        public void AcceptRequest()
        {
            if (SelectedRequests == null) return;

            int? curatorId = null;
            if (BindMe)
            {
                curatorId = CurrentUser.Id;
            }
            else if (BindFor && SelectedCuratorForBind != null)
            {
                curatorId = SelectedCuratorForBind.Id;
            }

            SelectedRequests.Status = "Принята";
            SelectedRequests.ReviewerId = CurrentUser.Id;
            _requestService.Update(SelectedRequests);

            var newParticipant = new Participant()
            {
                Name = Name,
                Login = SelectedRequests.Login,
                Password = SelectedRequests.Password,
                CuratorCreateId = CurrentUser.Id,
                CuratorCreate = (Curator)CurrentUser.ClassUser,
                IsArchive = false,
            };

            _participantService.Add(newParticipant);
            _participantService.UpdateParticipantBindForCurator(_participantService.GetLast().Id, (int)curatorId, true);

            _dialogService.ShowMessage($"Заявка от '{Name}' принята. Участник создан.", "Успех");

            AcceptEnabled = false;
            RejectEnabled = false;
            ApplyFilters();
        }

        public void RejectRequest()
        {
            if (SelectedRequests == null) return;

            SelectedRequests.Status = "Отклонена";
            SelectedRequests.ReviewerId = CurrentUser.Id;
            _requestService.Update(SelectedRequests);

            _dialogService.ShowMessage($"Заявка от '{Name}' отклонена.", "Информация");

            AcceptEnabled = false;
            RejectEnabled = false;
            ApplyFilters();
        }

        public override void ApplyFilters()
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

            if (SelectedRequests != null && !RequestsList.Contains(SelectedRequests))
            {
                SelectedRequests = null;
            }
        }
    }
}