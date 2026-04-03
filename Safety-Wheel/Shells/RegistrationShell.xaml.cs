using CozyTest.ViewModels.CuratorVM.AdministrationVM;
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
    /// Логика взаимодействия для ShellRegistration.xaml
    /// </summary>
    public partial class RegistrationShell : UserControl
    {
        string passwordBox;
        string rePasswordBox;
        public RegistrationShell()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        { 
            passwordBox = PasswordB.Password;
            rePasswordBox = RePasswordB.Password;
            if (DataContext is RegistrationViewModel vm && passwordBox != null && rePasswordBox != null)
            {
                vm.Password = passwordBox;
                vm.RePassword = rePasswordBox;
            }
        }
    }
}
