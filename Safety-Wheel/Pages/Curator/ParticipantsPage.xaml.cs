using CozyTest.ForShellWindow;
using CozyTest.ViewModels.CuratorVM;
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

namespace CozyTest.Pages.Curator
{
    /// <summary>
    /// Логика взаимодействия для UsersPage.xaml
    /// </summary>
    public partial class ParticipantsPage : UserControl
    {
        private AdminPanelViewModel _viewModel = new();

        public ParticipantsPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
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
                ShellWindow window = new ShellWindow(new CreateEditParticipantAdmin(_viewModel, 0));
                window.Show();
            }
            else
            {
                ShellWindow window = new ShellWindow(new CreateEditParticipant(_viewModel, 0));
                window.Show();
            }
        }

        private void Button_EditParticipant(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.TypeUser == 1)
            {
                ShellWindow window = new ShellWindow(new CreateEditParticipantAdmin(_viewModel));
                window.Show();
            }
            else
            {
                ShellWindow window = new ShellWindow(new CreateEditParticipant(_viewModel));
                window.Show();
            }
        }
    }
}
