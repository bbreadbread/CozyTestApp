using CozyTest.Models;
using CozyTest.Pages.Curator;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class AuthorizationViewModel : BaseViewModel
    {
        private readonly ParticipantService _participantService = new();
        private readonly CuratorService _curatorService = new();

        private string _login = "123";
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _password = "123";
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand AuthCommand { get; }
        public ICommand RequestAccountCommand { get; }

        public AuthorizationViewModel(
            IDialogService dialogService,
            INavigationService navigationService) : base(navigationService, dialogService)
        {
            AuthCommand = new RelayCommand(_ => Authenticate(), _ => CanAuthenticate());
            RequestAccountCommand = new RelayCommand(_ => RequestAccount());
        }

        private bool CanAuthenticate()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }

        private void Authenticate()
        {
            _participantService.GetAllParticipants();
            _curatorService.GetAll();

            var participant = _participantService.Participants.FirstOrDefault(s =>
                s.Login == Login && s.Password == Password);

            if (participant != null)
            {
                SetCurrentUser(participant, 3);
                NavigateToMain();
                return;
            }

            var curator = _curatorService.Curators.FirstOrDefault(t =>
                t.Login == Login && t.Password == Password);

            if (curator != null)
            {
                var fullCurator = _curatorService.GetById(curator.Id);
                int userType = curator.IsAdmin == true ? 1 : 2;
                SetCurrentUser(fullCurator, userType);
                NavigateToMain();
                return;
            }

            _dialogService.ShowMessage("Неверный логин или пароль.", "Ошибка");
        }

        private void SetCurrentUser(object user, int type)
        {
            CurrentUser.ClassUser = user;
            CurrentUser.TypeUser = (byte)type;
            CurrentUser.Id = user is Participant p ? p.Id : ((Curator)user).Id;
            CurrentUser.Name = user is Participant part ? part.Name : ((Curator)user).Name;

            if (user is Curator c)
                CurrentUser.Login = c.Login;
        }

        private void NavigateToMain()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.VM.InitAfterLogin();
                mainWindow.UpdateUserName(CurrentUser.Name);
            }

            _navigationService.NavigateTo(new AdminPanelViewModel(_dialogService, _navigationService));
        }

        private void RequestAccount()
        {
            var vm = new RegistrationViewModel(_dialogService, _navigationService);
            _dialogService.ShowWindow<ShellWindow>(vm);
        }
    }
}