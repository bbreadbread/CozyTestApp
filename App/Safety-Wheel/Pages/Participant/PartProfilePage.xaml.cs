using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using CozyTest.ViewModels.ParticipantVM;
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

namespace CozyTest.Pages.Participant
{
    /// <summary>
    /// Логика взаимодействия для PartProfilePage.xaml
    /// </summary>
    public partial class PartProfilePage : UserControl
    {
        public PartProfilePage()
        {
            InitializeComponent();
        }


        private void OldPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is PartProfileViewModel vm && sender is PasswordBox pb)
            {
                vm.OldPassword = pb.Password;
            }
        }

        private void NewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is PartProfileViewModel vm && sender is PasswordBox pb)
            {
                vm.NewPassword = pb.Password;
            }
        }

        private void ReNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is PartProfileViewModel vm && sender is PasswordBox pb)
            {
                vm.reNewPassword = pb.Password;
            }
        }
    }
}
