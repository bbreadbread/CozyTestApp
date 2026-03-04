using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Converters;
using Microsoft.IdentityModel.Tokens;
using Safety_Wheel.Models;
using Safety_Wheel.Pages.Participant;
using Safety_Wheel.Pages.Curator;
using Safety_Wheel.Services;
using Safety_Wheel.ViewModels;

namespace Safety_Wheel
{
    public partial class MainWindow : MetroWindow
    {
        private readonly CuratorService teacherService = new();
        private readonly ParticipantService participantService = new();
        private readonly AttemptService attemptService = new();

        private Participant _selectedParticipant;

        public MainViewModelCurator VM { get; }

        public MainWindow()
        {
            InitializeComponent();
            CuratorLoginRule.CuratorService = teacherService;
            CuratorLoginRule.OriginalLogin = CurrentUser.Login;
            VM = new MainViewModelCurator();
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
                case PartTest:
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
                            PartTest._isTestActivated = false;
                            MainFrame.Navigate(new PartHomePage());
                        }

                        break;
                    }

                case CuratorMainPage:
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
                            PartTest._isTestActivated = false;
                            Clear();
                            MainFrame.Navigate(new MainPage());
                        }

                        break;
                    }

                case PartHomePage:
                    {
                        PartTest._isTestActivated = false;
                        MainFrame.Navigate(new PartHomePage());
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
                PartTest._isTestActivated = false;
                Clear();
                MainFrame.Navigate(new MainPage());
            }
        }

        private void Clear()
        {
            CurrentUser.Clear();
            UpdateUserName("");
        }

        public void OpenCuratorManagerFlyout()
        {
            ReloadParticipants();
            CuratorManagerFlyout.IsOpen = true;
        }

        private void CuratorManagerFlyout_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (!CuratorManagerFlyout.IsOpen) return;

            VM.ParticipantName = VM.ParticipantLogin = VM.ParticipantPassword = "";
            VM.CuratorName = VM.CuratorLogin = VM.CuratorPassword = "";

            ParticipantLoginRule.OriginalLogin = null;
            CuratorLoginRule.OriginalLogin = CurrentUser.Login;

            ReloadParticipants();
            LoadCuratorData();
        }

        private void ReloadParticipants()
        {
            using var db = new CozyTestContext();
            ParticipantsGrid.ItemsSource = db.Participants
                                        .Where(s => s.CuratorCreateId == CurrentUser.Id)
                                        .ToList();

            VM?.ReloadParticipants();
        }


        private void AddParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidatePanel(ParticipantInputsPanel))
                return;

            if (String.IsNullOrEmpty(PasswordTextBox.Text) || String.IsNullOrEmpty(LoginTextBox.Text) || String.IsNullOrEmpty(NameTextBox.Text))
                return;

            var participant = new Participant
            {
                Name = VM.ParticipantName,
                Login = VM.ParticipantLogin,
                Password = VM.ParticipantPassword,
                CuratorCreateId = CurrentUser.Id
            };

            participantService.Add(participant);
            ClearInputs();
            ReloadParticipants();
        }
        private void SaveParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedParticipant == null) return;

            foreach (var tb in ParticipantInputsPanel.Children.OfType<TextBox>())
                tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

            if (!ValidatePanel(ParticipantInputsPanel))
                return;

            var participant = participantService.GetCurrentParticipant(_selectedParticipant.Id);
            if (participant == null) return;

            participant.Name = VM.ParticipantName;
            participant.Login = VM.ParticipantLogin;
            participant.Password = VM.ParticipantPassword;

            participantService.Update(participant);
            ReloadParticipants();
        }
        private void DeleteParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedParticipant == null) return;

            if (MessageBox.Show("Удалить ученика?", "Подтверждение",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            Participant participant = participantService.GetCurrentParticipant(_selectedParticipant.Id);
            participantService.Remove(participant);

            ClearInputs();
            _selectedParticipant = null;
            ReloadParticipants();
        }

        private void ParticipantsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParticipantsGrid.SelectedItem is Participant participant)
            {
                _selectedParticipant = participant;
                NameTextBox.Text = participant.Name;
                LoginTextBox.Text = participant.Login;
                PasswordTextBox.Text = participant.Password;

                ParticipantLoginRule.OriginalLogin = participant.Login;
            }
            else
            {
                ParticipantLoginRule.OriginalLogin = null;
                ClearInputs();
            }
        }

        private void ClearInputs()
        {
            VM.ParticipantName = "";
            VM.ParticipantLogin = "";
            VM.ParticipantPassword = "";
            ParticipantLoginRule.OriginalLogin = null;

            ParticipantsGrid.SelectedItem = null;
            _selectedParticipant = null;
        }

        private void Calendar_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            if (sender is Calendar calendar && calendar.DisplayMode != CalendarMode.Year)
                calendar.DisplayMode = CalendarMode.Year;
        }

        private void LoadCuratorData()
        {
            var teacher = teacherService.GetCuratorById(CurrentUser.Id);

            VM.CuratorName = teacher.Name;
            VM.CuratorLogin = teacher.Login;
            VM.CuratorPassword = teacher.Password;
        }

        private void UpdateCurator_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tb in CuratorInputsPanel.Children.OfType<TextBox>())
                tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

            if (!ValidatePanel(CuratorInputsPanel))
                return;

            var teacher = teacherService.GetCuratorById(CurrentUser.Id);
            teacher.Name = VM.CuratorName;
            teacher.Login = VM.CuratorLogin;
            teacher.Password = VM.CuratorPassword;

            teacherService.Update(teacher);

            CurrentUser.Login = teacher.Login;
            CuratorLoginRule.OriginalLogin = teacher.Login;
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
            if (PartTest._isTestActivated == true)
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
            if (PartTest._isTestActivated == true) attemptService.Remove(PartTest._attempt);
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