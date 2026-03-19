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

namespace CozyTest.ForShellWindow
{
    /// <summary>
    /// Логика взаимодействия для CreateEditParticipantAdmin.xaml
    /// </summary>
    public partial class CreateEditParticipantAdmin : UserControl
    {
        AdminPanelViewModel adminPanelViewModel;
        public CreateEditParticipantAdmin(AdminPanelViewModel viewModel, int? isNew = null)
        {
            adminPanelViewModel = viewModel;
            InitializeComponent();
            if (isNew != null)
            {
                adminPanelViewModel.SelectedParticipant = null;
            }
            DataContext = adminPanelViewModel;
        }

        private void Button_Bind(object sender, RoutedEventArgs e)
        {
            ShellWindow window = new ShellWindow(new BindGroupForUser(adminPanelViewModel));
            window.Show();
        }
    }
}
