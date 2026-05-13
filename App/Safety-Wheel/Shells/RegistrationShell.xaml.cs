using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using System.Windows;
using System.Windows.Controls;

namespace CozyTest.ForShellWindow
{
    public partial class RegistrationShell : UserControl
    {
        public RegistrationShell()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegistrationViewModel vm)
            {
                vm.Password = PasswordB.Password;
                vm.RePassword = RePasswordB.Password;
            }
        }
    }
}