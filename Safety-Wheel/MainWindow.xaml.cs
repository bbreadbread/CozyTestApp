using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Converters;
using Microsoft.IdentityModel.Tokens;
using Safety_Wheel.Models;
using Safety_Wheel.Pages.Student;
using Safety_Wheel.Pages.Teacher;
using Safety_Wheel.Services;
using Safety_Wheel.ViewModels;

namespace Safety_Wheel
{
    public partial class MainWindow : MetroWindow
    {
        private readonly TeacherService teacherService = new();
        private readonly StudentService studentService = new();
        private readonly AttemptService attemptService = new();

        private Student _selectedStudent;


        public MainViewModel VM { get; }

        public MainWindow()
        {
            InitializeComponent();
            TeacherLoginRule.TeacherService = teacherService;
            TeacherLoginRule.OriginalLogin = CurrentUser.Login;
            VM = new MainViewModel();
            DataContext = VM;
            InitMonths();
        }

        public void UpdateUserName(string userName)
        {
            VM.UserFullName = userName ?? string.Empty;
        }


        private void BackImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!MainFrame.CanGoBack) return;

            var currentPage = MainFrame.Content as Page;

            switch (currentPage)
            {
                case StudTest:
                    {
                        var confirm = new ClosedWindow(
                            "Вы намерены вернуться к выбору теста.",
                            "Тест будет считаться завершенным.")
                        {
                            Owner = this,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };

                        if (confirm.ShowDialog() == true)
                        {
                            StudTest._isTestActivated = false;
                            MainFrame.Navigate(new StudSelectedTestsPage(StudHomePage.NameDiscipline, StudHomePage.thisStudent));
                        }

                        break;
                    }

                case StudHomePage:
                case TeacherMainPage:
                    {
                        var confirm = new ClosedWindow(
                            "Вы намерены выйти из аккаунта",
                            null)
                        {
                            Owner = this,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };

                        if (confirm.ShowDialog() == true)
                        {
                            StudTest._isTestActivated = false;
                            Clear();
                            MainFrame.Navigate(new MainPage());
                        }

                        break;
                    }

                case StudSelectedTestsPage:
                    {
                        StudTest._isTestActivated = false;
                        MainFrame.Navigate(new StudHomePage(CurrentUser.Id));
                        break;
                    }

                default:
                    MainFrame.GoBack();
                    break;
            }
        }


        private void ExitImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var confirm = new ClosedWindow("Вы намерены выйти из аккаунта.", "")
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (confirm.ShowDialog() == true)
            {
                StudTest._isTestActivated = false;
                Clear();
                MainFrame.Navigate(new MainPage());
            }
        }

        private void Clear()
        {
            CurrentUser.Clear();
            UpdateUserName("");
        }

        public void OpenTeacherManagerFlyout()
        {
            ReloadStudents();
            TeacherManagerFlyout.IsOpen = true;
        }

        private void TeacherManagerFlyout_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (!TeacherManagerFlyout.IsOpen) return;

            VM.StudentName = VM.StudentLogin = VM.StudentPassword = "";
            VM.TeacherName = VM.TeacherLogin = VM.TeacherPassword = "";

            StudentLoginRule.OriginalLogin = null;
            TeacherLoginRule.OriginalLogin = CurrentUser.Login;

            ReloadStudents();
            LoadTeacherData();
        }

        private void ReloadStudents()
        {
            using var db = new SafetyWheelContext();
            StudentsGrid.ItemsSource = db.Students
                                        .Where(s => s.TeachersId == CurrentUser.Id)
                                        .ToList();

            VM?.ReloadStudents();
        }


        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidatePanel(StudentInputsPanel))
                return;

            if (String.IsNullOrEmpty(PasswordTextBox.Text) || String.IsNullOrEmpty(LoginTextBox.Text) || String.IsNullOrEmpty(NameTextBox.Text))
                return;

            var student = new Student
            {
                Name = VM.StudentName,
                Login = VM.StudentLogin,
                Password = VM.StudentPassword,
                TeachersId = CurrentUser.Id
            };

            studentService.Add(student);
            ClearInputs();
            ReloadStudents();
        }
        private void SaveStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent == null) return;

            foreach (var tb in StudentInputsPanel.Children.OfType<TextBox>())
                tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

            if (!ValidatePanel(StudentInputsPanel))
                return;

            var student = studentService.GetCurrentStudent(_selectedStudent.Id);
            if (student == null) return;

            student.Name = VM.StudentName;
            student.Login = VM.StudentLogin;
            student.Password = VM.StudentPassword;

            studentService.Update(student);
            ReloadStudents();
        }
        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent == null) return;

            if (MessageBox.Show("Удалить ученика?", "Подтверждение",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            Student student = studentService.GetCurrentStudent(_selectedStudent.Id);
            studentService.Remove(student);

            ClearInputs();
            _selectedStudent = null;
            ReloadStudents();
        }

        private void StudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is Student student)
            {
                _selectedStudent = student;
                NameTextBox.Text = student.Name;
                LoginTextBox.Text = student.Login;
                PasswordTextBox.Text = student.Password;

                StudentLoginRule.OriginalLogin = student.Login;
            }
            else
            {
                StudentLoginRule.OriginalLogin = null;
                ClearInputs();
            }
        }

        private void ClearInputs()
        {
            VM.StudentName = "";
            VM.StudentLogin = "";
            VM.StudentPassword = "";
            StudentLoginRule.OriginalLogin = null;

            StudentsGrid.SelectedItem = null;
            _selectedStudent = null;
        }

        private void Calendar_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            if (sender is Calendar calendar && calendar.DisplayMode != CalendarMode.Year)
                calendar.DisplayMode = CalendarMode.Year;
        }

        private void LoadTeacherData()
        {
            var teacher = teacherService.GetTeacherById(CurrentUser.Id);

            VM.TeacherName = teacher.Name;
            VM.TeacherLogin = teacher.Login;
            VM.TeacherPassword = teacher.Password;
        }

        private void UpdateTeacher_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tb in TeacherInputsPanel.Children.OfType<TextBox>())
                tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

            if (!ValidatePanel(TeacherInputsPanel))
                return;

            var teacher = teacherService.GetTeacherById(CurrentUser.Id);
            teacher.Name = VM.TeacherName;
            teacher.Login = VM.TeacherLogin;
            teacher.Password = VM.TeacherPassword;

            teacherService.Update(teacher);

            CurrentUser.Login = teacher.Login;
            TeacherLoginRule.OriginalLogin = teacher.Login;
            VM.UserFullName = teacher.Name;

            MessageBox.Show("Данные преподавателя успешно обновлены",
                            "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        //месяцы

        private void InitMonths()
        {
            MonthComboBox.ItemsSource = new List<MonthItem>
            {
                new() { Number = 1, Name = "Январь" },
                new() { Number = 2, Name = "Февраль" },
                new() { Number = 3, Name = "Март" },
                new() { Number = 4, Name = "Апрель" },
                new() { Number = 5, Name = "Май" },
                new() { Number = 6, Name = "Июнь" },
                new() { Number = 7, Name = "Июль" },
                new() { Number = 8, Name = "Август" },
                new() { Number = 9, Name = "Сентябрь" },
                new() { Number = 10, Name = "Октябрь" },
                new() { Number = 11, Name = "Ноябрь" },
                new() { Number = 12, Name = "Декабрь" }
            };
        }

        private void YearTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void MonthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VM == null) return;
            if (MonthComboBox.SelectedItem is not MonthItem month) return;
            if (!int.TryParse(YearTextBox.Text, out int year)) return;

            VM.SelectedMonthDate = new DateTime(year, month.Number, 1);
        }
        //закртие
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var txt = "";
            if (StudTest._isTestActivated == true)
                txt = "Попытка аннулируется.";
            else txt = "Перед закрытием убедитесь, что сохранили прогресс";

            var confirm = new ClosedWindow("Вы намерены закрыть приложение", txt)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            if (confirm.ShowDialog() == false)
            {
                e.Cancel = true;
            }
            if (StudTest._isTestActivated == true) attemptService.Remove(StudTest._attempt);
        }
        private bool ValidatePanel(StackPanel panel)
        {
            var firstError = panel.Children
                                  .OfType<TextBox>()
                                  .FirstOrDefault(tb => Validation.GetHasError(tb));

            if (firstError == null) return true;

            var msg = Validation.GetErrors(firstError).FirstOrDefault()?.ErrorContent ?? "Ошибка";
            MessageBox.Show((string)msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            firstError.Focus();
            return false;
        }

    }
    public class MonthItem
    {
        public int Number { get; set; }  
        public string Name { get; set; } 
    }

}