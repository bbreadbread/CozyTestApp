using MaterialDesignThemes.Wpf;
using Safety_Wheel.ForShellWindow;
using Safety_Wheel.ViewModels.CuratorVM;
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

namespace Safety_Wheel.Pages.Curator
{
    /// <summary>
    /// Логика взаимодействия для AdminPanelPage.xaml
    /// </summary>
    public partial class AdminPanelPage : Page
    {
        private AdminPanelViewModel _viewModel = new();
        public AdminPanelPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void TabItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TabItem tabItem && tabItem.Tag is string tag)
            {
                _viewModel.SelectedTab = Enum.Parse<AdminPanelViewModel.TabType>(tag);
            }
        }
        private void Button_SearchParticipant(object sender, RoutedEventArgs e)
        {
            ShellWindow window = new ShellWindow(new SearchParticipantShell());
            window.Show();
        }
        private void Button_AddParticipant(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.TypeUser == 1)
            {
                ShellWindow window = new ShellWindow(new CreateEditParticipantAdmin(_viewModel));
                window.Show();
            }
            else
            {
                ShellWindow window = new ShellWindow(new CreateEditParticipant());
                window.Show();
            }
        }
        private void Button_CreateParticipant(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.TypeUser == 1)
            {
                ShellWindow window = new ShellWindow(new CreateEditParticipantAdmin(_viewModel));
                window.Show();
            }
            else
            {
                ShellWindow window = new ShellWindow(new CreateEditParticipant());
                window.Show();
            }
        }
        private void Button_ArchiveParticipant(object sender, RoutedEventArgs e)
        {
            //
        }
        private void Button_AddCurator(object sender, RoutedEventArgs e)
        {
            ShellWindow window = new ShellWindow(new CreateEditParticipant());//надо куратора
            window.Show();
        }
        private void Button_AddGroup(object sender, RoutedEventArgs e)
        {
            ShellWindow window = new ShellWindow(new CreateEditGroup());
            window.Show();
        }
    }
}
