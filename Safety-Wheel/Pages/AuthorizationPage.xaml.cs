using Safety_Wheel.Pages.Curator;
using Safety_Wheel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Safety_Wheel.Models;
using WPFCustomMessageBox;
using Safety_Wheel.ForShellWindow;

namespace Safety_Wheel.Pages.Participant
{
    /// <summary>
    /// Логика взаимодействия для MainNavigation.xaml
    /// </summary>
    public partial class AuthorizationPage : Page
    {
        private ParticipantService _participantService = new();
        private CuratorService _teacherService = new();

        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void ButtonAuth_Click(object sender, RoutedEventArgs e)
        { 
            _teacherService.GetAll();
            string login = TbLogin.Text.Trim();
            string password = TbPassword.Password.Trim();

            _participantService.GetAllParticipants();
            var participant = _participantService.Participants.FirstOrDefault(s =>
                            s.Login == login && s.Password == password);
            if (participant != null)
            {
                CurrentUser.TypeUser = 3;
                CurrentUser.Id = participant.Id;
                CurrentUser.Name = participant.Name ?? string.Empty;

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.VM.InitAfterLogin();
                    mainWindow.UpdateUserName(CurrentUser.Name);
                }

                NavigationService.Navigate(new MainNavigation());
                return;
            }

            var curator = _teacherService.Curators.FirstOrDefault(t => t.Login == login && t.Password == password);
            if (curator != null)
            {
                if (curator.IsAdmin == true)
                    CurrentUser.TypeUser = 1;
                else CurrentUser.TypeUser = 1;
                CurrentUser.Id = curator.Id;
                CurrentUser.Name = curator.Name ?? string.Empty;
                CurrentUser.Login = _teacherService.GetCuratorById(curator.Id).Login;

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.VM.InitAfterLogin();
                    mainWindow.UpdateUserName(CurrentUser.Name);
                }
                
                NavigationService.Navigate(new MainNavigation());
                return;
            }

            CustomMessageBox.Show("Неверный логин или пароль.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        }

        private void RequestAccount_Click(object sender, MouseButtonEventArgs e)
        {
            var window = new ShellWindow(new RegistrationShell());
            window.Show();
        }
    }
}
