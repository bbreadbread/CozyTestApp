using RegistrationTeacherSafetyWheel.Models;
using RegistrationTeacherSafetyWheel.ViewModels;
using RegistrationTeacherSafetyWheel.Service;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RegistrationTeacherSafetyWheel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Teacher? selectedItem { get; set; } = null;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = TVM;
            LoadTeacherToList();

        }
        RegisterTeacherViewModel TVM { get; set; } = new();
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
            if (TVM.originalTeacher != null)
            {
                PasswordBox.Password = TVM.originalTeacher.Password;
                ConfirmPasswordBox.Password = TVM.originalTeacher.Password;
            }
            else
            {
                PasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;
            }
        }
        public void LoadTeacherToList()
        {
            TVM.Load();
        }
    }
}