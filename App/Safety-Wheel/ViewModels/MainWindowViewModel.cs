using CozyTest.Pages.Curator;
using CozyTest.Pages.Participant;
using CozyTest.Services;
using CozyTest.Models;

using CozyTest.ViewModels.CuratorVM;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CozyTest.ForShellWindow;

namespace CozyTest.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ParticipantService _participantService;
        private CuratorService _curatorService;

        private object _currentPage;
         private string _userFullName;
        public string UserFullName
        {
            get => _userFullName;
            set => SetProperty(ref _userFullName, value);
        }

         private bool _adminRoleOn = false;
        public bool AdminRoleOn
        {
            get => _adminRoleOn;
            set
            {
                CurrentUser.AdminModeOn = value;
                SetProperty(ref _adminRoleOn, value);
            }
        }
        public object CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }


        public ICommand LogoutCommand { get; }
        public ICommand ShowLogsCommand { get; }

        public MainWindowViewModel(INavigationService navigationService, IDialogService dialogService,
             ParticipantService participantService,
             CuratorService curatorService, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _participantService = participantService;
            _curatorService = curatorService;


            LogoutCommand = new RelayCommand(_ => Logout());
            ShowLogsCommand = new RelayCommand(_ => ShowLogs());

            SetAuthorizationPage();
        }

        public void SetAuthorizationPage()
        {
            var authViewModel = App.Services.GetRequiredService<AuthorizationViewModel>();
            authViewModel.LoginSuccess += OnLoginSuccess;
            CurrentPage = authViewModel;
        }

        public void ShowLogs()
        {
            var logsViewModel = App.Services.GetRequiredService<LogsViewModel>();
            _dialogService.ShowWindow<ShellWindow>(logsViewModel);
        }

        private void OnLoginSuccess(object sender, EventArgs e)
        {
            SetMainNavigationPage();
        }

        public async void SetMainNavigationPage()
        {
            var mainNav = App.Services.GetRequiredService<MainViewModel>();
            CurrentPage = mainNav;

            await mainNav.InitAfterLogin();
        }

        private void Logout()
        {
            UserFullName = "";
            CurrentUser.Clear();
            SetAuthorizationPage();

            ListRoleCurrentUser.Clear();

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.RefreshUserRole();
            }
        }

        private ObservableCollection<object> _listRoleCurrentUser = new();

        public ObservableCollection<object> ListRoleCurrentUser
        {
            get => _listRoleCurrentUser;
            set
            {
                SetProperty(ref _listRoleCurrentUser, value);
            }
        }


        private object _SelectedRoleCurrentUser = new();
        public object SelectedRoleCurrentUser
        {
            get => _SelectedRoleCurrentUser;
            set
            {
                System.Diagnostics.Debug.WriteLine($"=== SelectedRoleCurrentUser SETTER ===");
                System.Diagnostics.Debug.WriteLine($"Old value: {_SelectedRoleCurrentUser?.GetType().Name ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"New value: {value?.GetType().Name ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"New value ToString: {value?.ToString() ?? "null"}");

                if (SetProperty(ref _SelectedRoleCurrentUser, value))
                {
                    System.Diagnostics.Debug.WriteLine($"Property changed, calling SwitchRole");
                    SwitchRole();
                }
            }
        }

        public async Task RefreshAfterRoleSwitch()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var mainNav = App.Services.GetRequiredService<MainViewModel>();
                CurrentPage = mainNav;
                await mainNav.InitAfterLogin();
                mainWindow.UpdateUserName(CurrentUser.Name);
            }
        }

        public async void SwitchRole()
        {
            if (SelectedRoleCurrentUser == null) return;

            await _curatorService.GetAllAsync();
            await _participantService.GetAllParticipantsAsync();

            if (SelectedRoleCurrentUser is Curator curator)
            {
                AdminRoleOn = false;
                CurrentUser.ClassUser = await _curatorService.GetById(curator.Id);
                CurrentUser.TypeUser = (byte)(curator.IsAdmin == true ? 1 : 2);
                CurrentUser.Id = curator.Id;
                CurrentUser.Name = curator.Name ?? string.Empty;
                var fullCurator = await _curatorService.GetById(curator.Id);
                CurrentUser.Login = fullCurator.Login;
            }
            else if (SelectedRoleCurrentUser is Participant participant)
            {
                CurrentUser.ClassUser = participant;
                CurrentUser.TypeUser = 3;
                CurrentUser.Id = participant.Id;
                CurrentUser.Name = participant.Name ?? string.Empty;
                var fullParticipant = await _participantService.GetByIdAsync(participant.Id);
                CurrentUser.Login = fullParticipant.Login;
            }

            await RefreshAfterRoleSwitch();
        }
    }
}