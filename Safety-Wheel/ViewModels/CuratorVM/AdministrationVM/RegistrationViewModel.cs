using ControlzEx.Standard;
using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class RegistrationViewModel : BaseViewModel
    {
        public override string WindowTitle => "Отправка заявки на регистрацию";

        public string _name;
        public string _login;
        public string _password;
        public string _repassword;
        public RequestService _requestService = new RequestService();

        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(Name); } }
        public string Login { get { return _login; } set { _login = value; OnPropertyChanged(Login); } }
        public string Password { get { return _password; } set { _password = value; OnPropertyChanged(Password); } }
        public string RePassword { get { return _repassword; } set { _repassword = value; OnPropertyChanged(RePassword); } }

        public RelayCommand SendRequestCommand { get; }

        public RegistrationViewModel(
            IDialogService dialogService,
            INavigationService navigationService) : base(navigationService, dialogService)
        {
            SendRequestCommand = new RelayCommand(_ => SendRequest());
        }

        public void SendRequest()
        {
            _requestService.Add(
                new Models.Requests()
                {
                    Name = this.Name,
                    Login = this.Login,
                    Password = this.Password,
                    Status = "Ожидает подтверждения",
                });
        }
    }
}
