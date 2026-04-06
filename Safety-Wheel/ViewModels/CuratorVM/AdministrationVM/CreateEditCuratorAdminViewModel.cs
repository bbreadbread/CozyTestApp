using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CreateEditCuratorAdminViewModel : BaseAdminViewModel
    {
        CuratorsViewModel _CuratorsViewModel;
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

        public CreateEditCuratorAdminViewModel(IDialogService dialogService, INavigationService navigationService, CuratorsViewModel CuratorsViewModel) : base(dialogService, navigationService)
        {
            _CuratorsViewModel = CuratorsViewModel;
            SaveCuratorCommand = new RelayCommand(_ => SaveCurator());

            if (_CuratorsViewModel.SelectedCurator != null)
                LoadSelectedCurator();
        }

        public void LoadSelectedCurator()
        {
            NameCurator = _CuratorsViewModel.SelectedCurator.Name;
            LoginCurator = _CuratorsViewModel.SelectedCurator.Login;
            PasswordCurator = _CuratorsViewModel.SelectedCurator.Password;
        }
        public void SaveCurator()
        {
            int? curatorId = null;

            if (_CuratorsViewModel.SelectedCurator != null)
            {
                var cur = new Curator()
                {
                    Id = _CuratorsViewModel.SelectedCurator.Id,
                    Name = NameCurator,
                    Login = LoginCurator,
                    Password = PasswordCurator,
                    IsArchive = _CuratorsViewModel.SelectedCurator.IsArchive,
                    IsAdmin = _CuratorsViewModel.SelectedCurator.IsAdmin,
                };
                _curatorService.Update(cur);

                _CuratorsViewModel.SelectedCurator.Name = NameCurator;
                _CuratorsViewModel.SelectedCurator.Login = LoginCurator;
                _CuratorsViewModel.SelectedCurator.Password = PasswordCurator;
            }
            else
            {
                var newC = new Curator()
                {
                    Name = NameCurator,
                    Login = LoginCurator,
                    Password = PasswordCurator,
                    IsArchive = false,
                    IsAdmin = false,
                    ParticipantProfileId = null,
                };
                _curatorService.Add(newC);

                var newP = new Participant()
                {
                    Name = NameCurator,
                    Login = LoginCurator,
                    Password = PasswordCurator,
                    CuratorCreateId = CurrentUser.Id,
                    CuratorCreate = (Curator)CurrentUser.ClassUser,
                    IsArchive = false,
                };
                _participantService.Add(newP);

                newC= _curatorService.GetLast();
                newC.ParticipantProfileId = _participantService.GetLast().Id;
                
                _curatorService.Update(newC);
                _CuratorsViewModel.ReloadCurators();
            }
        }

        public override void ApplyFilters()
        {
            throw new NotImplementedException();
        }

    }
}
