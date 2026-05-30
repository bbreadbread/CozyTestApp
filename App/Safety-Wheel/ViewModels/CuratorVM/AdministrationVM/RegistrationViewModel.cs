using CozyTest.Services;
using System;
using System.Linq;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class RegistrationViewModel : BaseViewModel
    {
        public override string WindowTitle => "Отправка заявки на регистрацию";

        private string _name;
        private string _login;
        private string _password;
        private string _repassword;
        private readonly RequestService _requestService;
        private readonly ParticipantService _participantService;
        private readonly CuratorService _curatorService;
        private readonly IDialogService _dialogService;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string RePassword
        {
            get => _repassword;
            set { _repassword = value; OnPropertyChanged(); }
        }

        public ICommand SendRequestCommand { get; }

        public RegistrationViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            RequestService requestService,
            ParticipantService participantService,
            CuratorService curatorService, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _dialogService = dialogService;
            _requestService = requestService;
            _participantService = participantService;
            _curatorService = curatorService;
            SendRequestCommand = new RelayCommand(_ => SendRequest());
        }

        public async void SendRequest()
        {
            if (!ValidateInput())
                return;

            try
            {
                bool loginExists = await CheckLoginExistsAsync(Login);

                if (loginExists)
                {
                    _dialogService.ShowMessage("Пользователь с таким логином уже существует", "Ошибка");
                    return;
                }

                await _requestService.AddAsync(new Models.Requests
                {
                    Name = this.Name,
                    Login = this.Login,
                    Password = this.Password,
                    Status = "Ожидает подтверждения",
                    DateTimeApplication = DateTime.Now
                });

                _dialogService.ShowMessage("Заявка отправлена. Результат подтверждения уточняйте у преподавателя", "Успех");

                await _logger.LogAsync(
                   whoMade: Name,
                   whoRole: "CozyTest.Models.Participant",
                   action: LogActionType.Authorization,
                   objectType: LogObjectType.Application,
                   objectName: Name
               );

                ClearForm();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка при отправке заявки: {ex.Message}", "Ошибка");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                _dialogService.ShowMessage("Введите ФИО", "Предупреждение");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                _dialogService.ShowMessage("Введите логин", "Предупреждение");
                return false;
            }

            if (Login.Length < 3)
            {
                _dialogService.ShowMessage("Логин должен содержать минимум 3 символа", "Предупреждение");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                _dialogService.ShowMessage("Введите пароль", "Предупреждение");
                return false;
            }

            if (Password.Length < 3)
            {
                _dialogService.ShowMessage("Пароль должен содержать минимум 4 символа", "Предупреждение");
                return false;
            }

            if (Password != RePassword)
            {
                _dialogService.ShowMessage("Пароли не совпадают", "Предупреждение");
                return false;
            }

            return true;
        }

        private async Task<bool> CheckLoginExistsAsync(string login)
        {
            await _participantService.InitializeAsync();
            await _curatorService.InitializeAsync();

            bool participantExists = _participantService.Participants
                .Any(p => p.Login != null && p.Login.Equals(login, StringComparison.OrdinalIgnoreCase));

            bool curatorExists = _curatorService.Curators
                .Any(c => c.Login != null && c.Login.Equals(login, StringComparison.OrdinalIgnoreCase));

            bool requestExists = _requestService.Requests
                .Any(r => r.Login != null && r.Login.Equals(login, StringComparison.OrdinalIgnoreCase)
                          && r.Status != "Отклонена");

            return participantExists || curatorExists || requestExists;
        }

        private void ClearForm()
        {
            Name = "";
            Login = "";
            Password = "";
            RePassword = "";
        }
    }
}