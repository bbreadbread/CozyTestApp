using Microsoft.Win32;
using Safety_Wheel.Models;
using Safety_Wheel.ViewModels.CreateTestsVM;
using System.Windows;
using System.Windows.Controls;

namespace Safety_Wheel.Pages.Teacher
{
    public partial class TeacherCreateTestsPage : UserControl
    {
        Test? _test = null;
        public TeacherCreateTestsPage()
        {
            InitializeComponent();
        }
        public TeacherCreateTestsPage(Test? test)
        {
            _test = test;
            InitializeComponent();
        }

        private void AddQuestionImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is QuestionCreateViewModel qvm)
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp",
                    InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images")
                };

                if (dlg.ShowDialog() == true)
                {
                    string imagesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!dlg.FileName.StartsWith(imagesPath, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Изображение должно находиться в папке Images проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string relativePath = dlg.FileName.Substring(imagesPath.Length + 1);
                    relativePath = relativePath.Replace('\\', '/');

                    qvm.SetQuestionImage("Images/" + relativePath);
                }
            }
        }

        private void AddOptionImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is OptionCreateViewModel ovm)
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp",
                    InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images")
                };

                if (dlg.ShowDialog() == true)
                {
                    string imagesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!dlg.FileName.StartsWith(imagesPath, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Изображение должно находиться в папке Images проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string relativePath = dlg.FileName.Substring(imagesPath.Length + 1);
                    relativePath = relativePath.Replace('\\', '/');

                    ovm.SetOptionImage("Images/" + relativePath);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TeacherCreateTestViewModel vm)
                vm.Save();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_test != null)
            {
                DataContext = new TeacherCreateTestViewModel(_test);
            }
            else
            {
                DataContext = new TeacherCreateTestViewModel();
            }
        }
    }
}
