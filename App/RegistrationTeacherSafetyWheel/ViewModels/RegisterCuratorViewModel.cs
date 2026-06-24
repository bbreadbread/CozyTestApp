using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using RegistrationCuratorCozyTest.Models;
using RegistrationCuratorCozyTest.Service;

namespace RegistrationCuratorCozyTest.ViewModels
{
    public class RegisterCuratorViewModel : INotifyPropertyChanged
    {
        private readonly CuratorService _curatorService;
        private readonly ParticipantService _participantService;

        private Curator _selectedCurator;
        private string _name;
        private string _login;
        private string _password;
        private string _confirmPassword;
        private bool _isEditMode;
        public bool _isCanDelete = false;

        public ObservableCollection<Curator> Curators { get; }

        public Curator SelectedCurator
        {
            get => _selectedCurator;
            set
            {
                _selectedCurator = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public bool IsCanDelete
        {
            get => _isCanDelete;
            set
            {
                _isCanDelete = value;
                OnPropertyChanged();
            }
        }

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand RegisterCommand { get; }
        public RelayCommand UpdateCommand { get; }
        public RelayCommand RemoveCommand { get; }

        public RegisterCuratorViewModel()
        {
            _curatorService = new CuratorService();
            _participantService = new ParticipantService();

            Curators = new ObservableCollection<Curator>();

            RegisterCommand = new RelayCommand(Register);
            UpdateCommand = new RelayCommand(Update);
            RemoveCommand = new RelayCommand(Remove);

            LoadCurators();
        }

        public void SelectCurator()
        {
            if (SelectedCurator == null) return;

            Name = SelectedCurator.Name;
            Login = SelectedCurator.Login;
            Password = SelectedCurator.Password;
            _isEditMode = true;
        }

        private void Register()
        {
            if (!ValidateFields()) return;
            if (_curatorService.UserExistsByLogin(Login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return;
            }

            var participant = new Participant
            {
                Name = Name,
                Login = Login + "_p",
                Password = Password,
                CuratorCreateId = 0,
                IsArchive = false
            };

            _participantService.Add(participant);
            var lastParticipant = _participantService.GetLast();

            var curator = new Curator
            {
                Name = Name,
                Login = Login,
                Password = Password,
                IsArchive = false,
                IsAdmin = true,
                ParticipantProfileId = lastParticipant.Id
            };

            _curatorService.Add(curator);

            MessageBox.Show("Куратор зарегистрирован");
            ClearFields();
            LoadCurators();
        }

        private void Update()
        {
            if (!ValidateFields()) return;
            if (SelectedCurator == null)
            {
                MessageBox.Show("Выберите куратора для редактирования");
                return;
            }

            var curator = new Curator
            {
                Id = SelectedCurator.Id,
                Name = Name,
                Login = Login,
                Password = Password
            };

            _curatorService.Update(curator);

            MessageBox.Show("Данные обновлены");
            ClearFields();
            LoadCurators();
        }

        private void Remove()
        {
            if (SelectedCurator == null)
            {
                MessageBox.Show("Выберите куратора для удаления");
                return;
            }

            if (MessageBox.Show($"Удалить {SelectedCurator.Name}?\n\nВНИМАНИЕ! Будут удалены все связанные данные: ученики и результаты тестов.",
                "Подтверждение удаления", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _curatorService.Remove(SelectedCurator);
                ClearFields();
                LoadCurators();
            }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Заполните поле 'Имя Фамилия'");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                MessageBox.Show("Заполните поле 'Логин'");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Заполните поле 'Пароль'");
                return false;
            }

            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Пароли не совпадают");
                return false;
            }

            return true;
        }

        private void LoadCurators()
        {
            Curators.Clear();
            foreach (var curator in _curatorService.Curators)
            {
                Curators.Add(curator);
            }
        }

        private void ClearFields()
        {
            Name = string.Empty;
            Login = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            SelectedCurator = null;
            _isEditMode = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}