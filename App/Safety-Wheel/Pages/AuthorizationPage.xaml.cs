using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Windows;
using System.Windows.Controls;

namespace CozyTest.Pages.Participant
{
    public partial class AuthorizationPage : UserControl
    {
        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthorizationViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }
    }
}