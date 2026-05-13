using CozyTest.Models;
using CozyTest.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class AuthorizationViewModel : BaseViewModel
    {
        private readonly ParticipantService _participantService;
        private readonly CuratorService _curatorService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;

        public event EventHandler LoginSuccess;

        private string _login;
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand AuthCommand { get; }
        public ICommand RequestAccountCommand { get; }

        public AuthorizationViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ParticipantService participantService,
            CuratorService curatorService,
            IServiceProvider serviceProvider) : base(navigationService, dialogService)
        {
            _participantService = participantService ?? throw new ArgumentNullException(nameof(participantService));
            _curatorService = curatorService ?? throw new ArgumentNullException(nameof(curatorService));
            _dialogService = dialogService;
            _serviceProvider = serviceProvider;

            AuthCommand = new RelayCommand(_ => _ = AuthenticateAsync(), _ => CanAuthenticate());
            RequestAccountCommand = new RelayCommand(_ => RequestAccount());
        }

        private bool CanAuthenticate()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task AuthenticateAsync()
        {
            try
            {
                await _participantService.InitializeAsync();
                await _curatorService.InitializeAsync();

                var participant = _participantService.Participants.FirstOrDefault(s =>
                    s.Login == Login && s.Password == Password);

                if (participant != null)
                {
                    SetCurrentUser(participant, 3);
                    await AfterLogin();
                    return;
                }

                var curator = _curatorService.Curators.FirstOrDefault(t =>
                    t.Login == Login && t.Password == Password);

                if (curator != null)
                {
                    var fullCurator = await _curatorService.GetById(curator.Id);
                    int userType = curator.IsAdmin == true ? 1 : 2;
                    SetCurrentUser(fullCurator, userType);
                    await AfterLogin();
                    return;
                }

                _dialogService.ShowMessage("Неверный логин или пароль.", "Ошибка");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка авторизации: {ex.Message}", "Ошибка");
            }
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

        private async Task AfterLogin()
        {
            try
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.UpdateUserName(CurrentUser.Name);
                    mainWindow.VM.SetMainNavigationPage();
                    mainWindow.VM.ListRoleCurrentUser.Clear();

                    if (CurrentUser.TypeUser == 1 || CurrentUser.TypeUser == 2) 
                    {
                        mainWindow.VM.ListRoleCurrentUser.Add(await _curatorService.GetById(CurrentUser.Id));

                        var participant = await _participantService.GetBindAccPartByIdCurAsync(CurrentUser.Id);
                        if (participant != null)
                            mainWindow.VM.ListRoleCurrentUser.Add(participant);
                    }
                    else if (CurrentUser.TypeUser == 3)
                    {
                        var participant = await _participantService.GetByIdAsync(CurrentUser.Id);
                        if (participant != null)
                            mainWindow.VM.ListRoleCurrentUser.Add(participant);
                    }

                    if (mainWindow.VM.ListRoleCurrentUser.Count > 0)
                    {
                        mainWindow.VM.SelectedRoleCurrentUser = mainWindow.VM.ListRoleCurrentUser[0];
                        mainWindow.RefreshUserRole();
                    }
                }

                LoginSuccess?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void RequestAccount()
        {
            var vm = _serviceProvider.GetRequiredService<RegistrationViewModel>();
            _dialogService.ShowWindow<ShellWindow>(vm);
        }
    }
}