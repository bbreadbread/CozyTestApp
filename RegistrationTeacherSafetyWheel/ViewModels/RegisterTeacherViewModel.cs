using Azure;
using RegistrationTeacherSafetyWheel.Models;
using RegistrationTeacherSafetyWheel.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RegistrationTeacherSafetyWheel.ViewModels
{
    public class RegisterTeacherViewModel : INotifyPropertyChanged
    {
        private TeacherService _teacherService = new();
        public Teacher? selectedItem { get; set; } = null;
        public ObservableCollection<object> Items { get; set; } = new();

        public Teacher _selectedTeacher;
        public Teacher originalTeacher { get; set; } = new();

        public Teacher _teacher = new ();
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

        public RegisterTeacherViewModel()
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
                (originalTeacher == null || originalTeacher.Login != Login))
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
            _teacher = new Teacher
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
            originalTeacher = selectedItem;
            if (originalTeacher != null)
            {
                _selectedTeacher = new Teacher
                {
                    Id = originalTeacher.Id,
                    Name = originalTeacher.Name,
                    Login = originalTeacher.Login,
                    Password = originalTeacher.Password
                };

                Name = _selectedTeacher.Name;
                Login = _selectedTeacher.Login;
                Password = _selectedTeacher.Password;
            }
        }
        public void Remove()
        {
            if (originalTeacher != null)
            {
                if (MessageBox.Show($"Удалить {originalTeacher.Name}?\n ВНИМАНИЕ! УДАЛЯЯ ПРЕПОДАВАТЕЛЯ, ВЫ ПОДТВЕРЖДАЕТЕ УДАЛЕНИЕ ВСЕХ ПРИВЯЗАННЫХ К НЕМУ ДАННЫХ!\n УДАЛЯТСЯ: ДАННЫЕ О УЧЕНИКАХ, ВСЕ ДАННЫЕ О ТЕСТАХ.", "Подтверждение",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _teacherService.Remove(originalTeacher);
                    ClearFields();
                }
            }
            Load();
        }
        public void Update()
        {
            if (Check() == false) return;

            if (originalTeacher != null)
            {
                _selectedTeacher = new Teacher
                {
                    Id = originalTeacher.Id,
                    Name = Name,
                    Login = Login,
                    Password = Password
                };

                _teacherService.Update( _selectedTeacher );
            }
            MessageBox.Show("Данные обновлены");
            Load();
        }

        public void Load()
        {
            Items.Clear();
            foreach (var p in _teacherService.Teachers)
                Items.Add(p);
        }

        private void ClearFields()
        {
            Name = string.Empty;
            Login = string.Empty;
            Password = "";
            ConfirmPassword = "";
            originalTeacher = null;
            selectedItem = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}

