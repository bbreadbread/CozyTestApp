using Azure;
using RegistrationCuratorCozyTest.Models;
using RegistrationCuratorCozyTest.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RegistrationCuratorCozyTest.ViewModels
{
    public class RegisterCuratorViewModel : INotifyPropertyChanged
    {
        private CuratorService _teacherService = new();
        public Curator? selectedItem { get; set; } = null;
        public ObservableCollection<object> Items { get; set; } = new();

        public Curator _selectedCurator;
        public Curator originalCurator { get; set; } = new();

        public Curator _teacher = new ();
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private string _confirmPassword;
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
        public RelayCommand SelectCommand { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand UpdateCommand { get; }

        public RegisterCuratorViewModel()
        {
            RegisterCommand = new RelayCommand(Register);
            SelectCommand = new RelayCommand(Select);
            RemoveCommand = new RelayCommand(Remove);
            UpdateCommand = new RelayCommand(Update);
        }

        private bool Check()
        {
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Login) ||
                string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Заполните все поля");
                return false;
            }

            if (_teacherService.UserExistsByLogin(Login) &&
                (originalCurator == null || originalCurator.Login != Login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return false;
            }

            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Пароли не совпадают");
                return false;
            }

            return true;
        }
        private void Register()
        {
            if (Check() == false) return;
            _teacher = new Curator
            {
                Name = Name,
                Login = Login,
                Password = Password
            };
            _teacherService.Add(_teacher);
            MessageBox.Show("Преподаватель зарегистрирован!");
            ClearFields();
            Load();
        }

        public void Select()
        {
            originalCurator = selectedItem;
            if (originalCurator != null)
            {
                _selectedCurator = new Curator
                {
                    Id = originalCurator.Id,
                    Name = originalCurator.Name,
                    Login = originalCurator.Login,
                    Password = originalCurator.Password
                };

                Name = _selectedCurator.Name;
                Login = _selectedCurator.Login;
                Password = _selectedCurator.Password;
            }
        }
        public void Remove()
        {
            if (originalCurator != null)
            {
                if (MessageBox.Show($"Удалить {originalCurator.Name}?\n ВНИМАНИЕ! УДАЛЯЯ ПРЕПОДАВАТЕЛЯ, ВЫ ПОДТВЕРЖДАЕТЕ УДАЛЕНИЕ ВСЕХ ПРИВЯЗАННЫХ К НЕМУ ДАННЫХ!\n УДАЛЯТСЯ: ДАННЫЕ О УЧЕНИКАХ, ВСЕ ДАННЫЕ О ТЕСТАХ.", "Подтверждение",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _teacherService.Remove(originalCurator);
                    ClearFields();
                }
            }
            Load();
        }
        public void Update()
        {
            if (Check() == false) return;

            if (originalCurator != null)
            {
                _selectedCurator = new Curator
                {
                    Id = originalCurator.Id,
                    Name = Name,
                    Login = Login,
                    Password = Password
                };

                _teacherService.Update( _selectedCurator );
            }
            MessageBox.Show("Данные обновлены");
            Load();
        }

        public void Load()
        {
            Items.Clear();
            foreach (var p in _teacherService.Curators)
                Items.Add(p);
        }

        private void ClearFields()
        {
            Name = string.Empty;
            Login = string.Empty;
            Password = "";
            ConfirmPassword = "";
            originalCurator = null;
            selectedItem = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}

