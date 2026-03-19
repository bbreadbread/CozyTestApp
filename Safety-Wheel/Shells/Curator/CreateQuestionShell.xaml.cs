using Microsoft.Win32;
using CozyTest.ViewModels.CreateTestsVM;
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
    /// Логика взаимодействия для CreateQuestion.xaml
    /// </summary>
    public partial class CreateQuestionShell : UserControl
    {
        public CreateQuestionShell()
        {
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
    }
}
