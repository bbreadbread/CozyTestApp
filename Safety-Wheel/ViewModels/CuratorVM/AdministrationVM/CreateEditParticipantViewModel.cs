using CozyTest.Services;
using CozyTest.Models;
using System;
using System.Collections.ObjectModel;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CreateEditParticipantViewModel : BaseAdminViewModel
    {
        ParticipantsViewModel _participantsViewModel;
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
        private Curator _selectedCuratorForBind;
        public Curator SelectedCuratorForBind
        {
            get => _selectedCuratorForBind;
            set => SetProperty(ref _selectedCuratorForBind, value);
        }
        private bool _bindMe = false;
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

        private bool _bindNone = true;
        public bool BindNone
        {
            get => _bindNone;
            set { _bindNone = value; OnPropertyChanged(nameof(BindNone)); }
        }

        private ObservableCollection<Curator> _curatorsList;
        public ObservableCollection<Curator> CuratorsList
        {
            get => _curatorsList;
            set => SetProperty(ref _curatorsList, value);
        }

        public RelayCommand SaveParticipantCommand { get; }

        public CreateEditParticipantViewModel(IDialogService dialogService, INavigationService navigationService, ParticipantsViewModel participantsViewModel) : base(dialogService, navigationService)
        {
            _participantsViewModel = participantsViewModel;
            SaveParticipantCommand = new RelayCommand(_ => SaveParticipant());
            CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);

            if (_participantsViewModel.SelectedParticipant != null)
                LoadSelectedParticipant();
        }

        public void LoadSelectedParticipant()
        {
            NameParticipant = _participantsViewModel.SelectedParticipant.Name;
            LoginParticipant = _participantsViewModel.SelectedParticipant.Login;
            PasswordParticipant = _participantsViewModel.SelectedParticipant.Password;
        }
        public void SaveParticipant()
        {
            int? curatorId = null;
            if (BindMe)
            {
                curatorId = CurrentUser.Id;
            }
            else if (BindFor && SelectedCuratorForBind != null)
            {
                curatorId = SelectedCuratorForBind.Id;
            }

            if (_participantsViewModel.SelectedParticipant != null)
            {
                var part = new Participant()
                {
                    Id = _participantsViewModel.SelectedParticipant.Id,
                    Name = NameParticipant,
                    Login = LoginParticipant,
                    Password = PasswordParticipant,
                    CuratorCreateId = _participantsViewModel.SelectedParticipant.CuratorCreateId,
                    CuratorCreate = _participantsViewModel.SelectedParticipant.CuratorCreate,
                    IsArchive = _participantsViewModel.SelectedParticipant.IsArchive,
                };
                _participantService.Update(part);
                _participantService.UpdateParticipantBindForCurator(part.Id, (int)curatorId, false);

                _participantsViewModel.SelectedParticipant.Name = NameParticipant;
                _participantsViewModel.SelectedParticipant.Login = LoginParticipant;
                _participantsViewModel.SelectedParticipant.Password = PasswordParticipant;
            }
            else
            {
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
                //_participantService.UpdateParticipantBindForCurator(_participantService.GetLast().Id, null, true);

                _participantsViewModel.ReloadParticipants();
            }
        }

        public override void ApplyFilters()
        {
            throw new NotImplementedException();
        }
    }
}