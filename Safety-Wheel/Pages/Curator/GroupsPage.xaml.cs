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
    /// Логика взаимодействия для GroupsPage.xaml
    /// </summary>
    public partial class GroupsPage : UserControl
    {
        private AdminPanelViewModel _viewModel = new();

        public GroupsPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void Button_AddGroup(object sender, RoutedEventArgs e)
        {
            ShellWindow window = new ShellWindow(new CreateEditGroup(_viewModel));

            window.Show();
        }
    }
}
