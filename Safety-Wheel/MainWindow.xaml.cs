using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using CozyTest.Pages;
using CozyTest.Pages.Curator;
using CozyTest.Pages.Participant;
using CozyTest.Services;
using CozyTest.ViewModels;

namespace CozyTest
{
    public partial class MainWindow : MetroWindow
    {
        private readonly AttemptService _attemptService = new();

        public MainWindow()
        {
            InitializeComponent();
            VM = new MainViewModel();
            DataContext = VM;

            VM.NavigationRequested += OnNavigationRequested;
            VM.ExitRequested += OnExitRequested;
            VM.ClearRequested += OnClearRequested;
        }

        public MainViewModel VM { get; set; }

        public void UpdateUserName(string userName)
        {
            VM.UserFullName = userName ?? string.Empty;
        }

        private void OnNavigationRequested(object sender, NavigationRequestEventArgs e)
        {
            var currentPage = MainFrame.Content as Page;
            HandleGoBack(currentPage);
        }

        private void HandleGoBack(Page currentPage)
        {
            switch (currentPage)
            {
                case PartTest:
                    ShowConfirm("Вы намерены вернуться к выбору теста.",
                               "Тест будет считаться завершенным.",
                               () => {
                                   PartTest._isTestActivated = false;
                                   MainFrame.Navigate(new PartHomePage());
                               });
                    break;

                case MainNavigation:
                    ShowConfirm("Вы намерены выйти из аккаунта", null,
                               () => {
                                   PartTest._isTestActivated = false;
                                   VM.RequestClear();
                                   MainFrame.Navigate(new AuthorizationPage());
                               });
                    break;

                case PartHomePage:
                    PartTest._isTestActivated = false;
                    MainFrame.Navigate(new PartHomePage());
                    break;

                default:
                    if (MainFrame.CanGoBack) MainFrame.GoBack();
                    break;
            }
        }

        private void OnExitRequested(object sender, EventArgs e)
        {
            ShowConfirm("Вы намерены выйти из аккаунта.", "",
                       () => {
                           PartTest._isTestActivated = false;
                           VM.RequestClear();
                           MainFrame.Navigate(new AuthorizationPage());
                       });
        }

        private void OnClearRequested(object sender, EventArgs e)
        {
            CurrentUser.Clear();
            UpdateUserName("");
        }

        private void ShowConfirm(string title, string subtitle, Action onConfirm)
        {
            var confirm = new ClosedWindow(title, subtitle)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (confirm.ShowDialog() == true) onConfirm();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var txt = PartTest._isTestActivated
                ? "Попытка аннулируется."
                : "Перед закрытием убедитесь, что сохранили прогресс";

            var confirm = new ClosedWindow("Вы намерены закрыть приложение", txt)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            if (confirm.ShowDialog() == false)
            {
                e.Cancel = true;
                return;
            }

            if (PartTest._isTestActivated) _attemptService.Remove(PartTest._attempt);
        }
    }
}