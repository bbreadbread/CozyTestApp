using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RegistrationCuratorCozyTest.ViewModels;

namespace RegistrationCuratorCozyTest
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterCuratorViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterCuratorViewModel viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RegisterCuratorViewModel viewModel)
            {
                viewModel.SelectCurator();

                if (viewModel.SelectedCurator != null)
                {
                    viewModel.IsCanDelete = true;
                    PasswordBox.Password = viewModel.SelectedCurator.Password;
                    ConfirmPasswordBox.Password = viewModel.SelectedCurator.Password;
                }
                else
                {
                    PasswordBox.Password = string.Empty;
                    ConfirmPasswordBox.Password = string.Empty;
                }
            }
        }
    }
}