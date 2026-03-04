using RegistrationCuratorCozyTest.Models;
using RegistrationCuratorCozyTest.ViewModels;
using RegistrationCuratorCozyTest.Service;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RegistrationCuratorCozyTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Curator? selectedItem { get; set; } = null;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = TVM;
            LoadCuratorToList();

        }
        RegisterCuratorViewModel TVM { get; set; } = new();
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TVM.Password = ((PasswordBox)sender).Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TVM.ConfirmPassword = ((PasswordBox)sender).Password;
        }

        private void FormsList_SelectDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TVM.Select();
            if (TVM.originalCurator != null)
            {
                PasswordBox.Password = TVM.originalCurator.Password;
                ConfirmPasswordBox.Password = TVM.originalCurator.Password;
            }
            else
            {
                PasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;
            }
        }
        public void LoadCuratorToList()
        {
            TVM.Load();
        }
    }
}