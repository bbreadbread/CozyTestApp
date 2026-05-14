using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CreateEditCuratorAdminViewModel : BaseAdminViewModel
    {
        public override string WindowTitle => "Управление пользователем";

        private readonly CuratorsViewModel _curatorsViewModel;
        private readonly CuratorService _curatorService;
        private readonly ParticipantService _participantService;

        private string _nameCur;
        private string _loginCur;
        private string _passwordCur;

        public string NameCurator
        {
            get => _nameCur;
            set => SetProperty(ref _nameCur, value);
        }
        public string LoginCurator
        {
            get => _loginCur;
            set => SetProperty(ref _loginCur, value);
        }
        public string PasswordCurator
        {
            get => _passwordCur;
            set => SetProperty(ref _passwordCur, value);
        }

        public RelayCommand SaveCuratorCommand { get; }

        public CreateEditCuratorAdminViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            CuratorsViewModel curatorsViewModel,
            CuratorService curatorService,
            ParticipantService participantService)
            : base(dialogService, navigationService, participantService, curatorService, null, null, null)
        {
            _curatorsViewModel = curatorsViewModel;
            _curatorService = curatorService;
            _participantService = participantService;

            SaveCuratorCommand = new RelayCommand(_ => _ = SaveCuratorAsync());

            if (_curatorsViewModel.SelectedCurator != null)
                LoadSelectedCurator();
        }

        public void LoadSelectedCurator()
        {
            NameCurator = _curatorsViewModel.SelectedCurator.Name;
            LoginCurator = _curatorsViewModel.SelectedCurator.Login;
            PasswordCurator = _curatorsViewModel.SelectedCurator.Password;
        }

        public async Task SaveCuratorAsync()
        {
            if (_curatorsViewModel.SelectedCurator != null)
            {
                var cur = new Curator()
                {
                    Id = _curatorsViewModel.SelectedCurator.Id,
                    Name = NameCurator,
                    Login = LoginCurator,
                    Password = PasswordCurator,
                    IsArchive = _curatorsViewModel.SelectedCurator.IsArchive,
                    IsAdmin = _curatorsViewModel.SelectedCurator.IsAdmin,
                };
                await _curatorService.UpdateAsync(cur);

                _curatorsViewModel.SelectedCurator.Name = NameCurator;
                _curatorsViewModel.SelectedCurator.Login = LoginCurator;
                _curatorsViewModel.SelectedCurator.Password = PasswordCurator;
            }
            else
            {
                var newP = new Participant()
                {
                    Name = NameCurator + "(Т)",
                    Login = LoginCurator + "_p",
                    Password = PasswordCurator,
                    CuratorCreateId = CurrentUser.Id,
                    CuratorCreate = (Curator)CurrentUser.ClassUser,
                    IsArchive = false,
                };
                await _participantService.AddAsync(newP);
                var partid = await _participantService.GetLastAsync();

                var newC = new Curator()
                {
                    Name = NameCurator,
                    Login = LoginCurator,
                    Password = PasswordCurator,
                    IsArchive = false,
                    IsAdmin = false,
                    ParticipantProfileId = partid.Id,
                };
                await _curatorService.AddAsync(newC);

                await _curatorsViewModel.ReloadCuratorsAsync();
            }
        }

        public override async Task ApplyFiltersAsync()
        {
            throw new NotImplementedException();
        }
    }
}